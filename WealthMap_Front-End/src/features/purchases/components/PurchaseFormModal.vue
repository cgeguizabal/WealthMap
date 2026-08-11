<script setup>
import { ref, watch, computed } from 'vue'
import {
  purchasesApi, PAYMENT_METHOD, PAYMENT_METHOD_OPTIONS, PURCHASE_CATEGORIES
} from '@/api/purchases.api'
import { accountsApi } from '@/api/accounts.api'
import { creditCardsApi } from '@/api/creditCards.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useMoney } from '@/composables/useMoney'
import { useDateTime } from '@/composables/useDateTime'
import { useAuthStore } from '@/stores/auth.store'

import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import StorePicker from '@/features/shared/components/StorePicker.vue'
import { useI18n } from '@/composables/useI18n'
import { useServerText } from '@/composables/useServerText'

const { t } = useI18n()
const { label: serverLabel } = useServerText()

/**
 * The api module keeps the option list because it owns the enum values; only
 * the wording is rebuilt here, where the locale is reactive.
 */
const methodOptions = computed(() =>
  PAYMENT_METHOD_OPTIONS.map((option) => ({
    ...option,
    label: serverLabel('paymentMethod', METHOD_ENUM_NAME[option.value]),
    note: t(METHOD_NOTE_KEY[option.value])
  }))
)

const METHOD_ENUM_NAME = {
  [PAYMENT_METHOD.DEBIT]: 'DebitAccount',
  [PAYMENT_METHOD.CREDIT]: 'CreditCard',
  [PAYMENT_METHOD.CASH]: 'Cash'
}

const METHOD_NOTE_KEY = {
  [PAYMENT_METHOD.DEBIT]: 'purchases.debitNote',
  [PAYMENT_METHOD.CREDIT]: 'purchases.creditNote',
  [PAYMENT_METHOD.CASH]: 'purchases.cashNote'
}

const props = defineProps({
  modelValue: { type: Boolean, default: false }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const { format } = useMoney()
const { toLocalInputValue, fromLocalInputValue } = useDateTime()
const auth = useAuthStore()

const accounts = ref([])
const cards = ref([])

const CURRENCIES = ['USD', 'MXN', 'EUR', 'GBP', 'CAD', 'BRL', 'COP', 'ARS']
  .map((code) => ({ value: code, label: code }))

/**
 * The value sent to the API stays the English name it stores; only the label
 * is translated, so switching language cannot change what gets saved.
 */
const categoryOptions = computed(() =>
  PURCHASE_CATEGORIES.map((name) => ({ value: name, label: serverLabel('category', name) }))
)

/**
 * Local wall-clock "now" for the datetime-local input. Built from local parts on
 * purpose — toISOString would hand the field a UTC time and silently shift what
 * the user sees by their offset.
 */
function now() {
  return toLocalInputValue(new Date())
}

function blank() {
  return {
    productName: '',
    amount: null,
    category: '',
    paymentMethod: PAYMENT_METHOD.DEBIT,
    accountId: null,
    creditCardId: null,
    currency: auth.currency,
    occurredAt: now(),
    storeId: null,
    notes: ''
  }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) => {
  // The API rejects a payload naming the wrong instrument for its method, so
  // only the relevant one is sent.
  const body = {
    productName: payload.productName,
    amount: payload.amount,
    category: payload.category,
    paymentMethod: payload.paymentMethod,
    // The input yields local wall-clock with no zone; the API stores UTC instants.
    occurredAt: fromLocalInputValue(payload.occurredAt),
    storeId: payload.storeId || null,
    notes: payload.notes || null,
    currency: null,
    accountId: null,
    creditCardId: null
  }

  if (payload.paymentMethod === PAYMENT_METHOD.DEBIT) body.accountId = payload.accountId
  if (payload.paymentMethod === PAYMENT_METHOD.CREDIT) body.creditCardId = payload.creditCardId
  if (payload.paymentMethod === PAYMENT_METHOD.CASH) body.currency = payload.currency

  return purchasesApi.create(body)
})

const isDebit = computed(() => values.paymentMethod === PAYMENT_METHOD.DEBIT)
const isCredit = computed(() => values.paymentMethod === PAYMENT_METHOD.CREDIT)
const isCash = computed(() => values.paymentMethod === PAYMENT_METHOD.CASH)

const accountOptions = computed(() =>
  accounts.value.map((account) => ({
    value: account.id,
    label: `${account.name} — ${format(account.balance, { currency: account.currency })}${account.isBlockedForSaving ? ' (blocked)' : ''}`,
    disabled: account.isBlockedForSaving
  }))
)

const cardOptions = computed(() =>
  cards.value.map((card) => ({
    value: card.id,
    label: `${card.cardName} — ${format(card.availableCredit, { currency: card.currency })} available`
  }))
)

/** Currency comes from the instrument except for cash, which has none. */
const effectiveCurrency = computed(() => {
  if (isDebit.value) return accounts.value.find((a) => a.id === values.accountId)?.currency ?? null
  if (isCredit.value) return cards.value.find((c) => c.id === values.creditCardId)?.currency ?? null
  return values.currency
})

/** Switching method must clear the instrument the previous one used. */
function chooseMethod(method) {
  values.paymentMethod = method
  values.accountId = null
  values.creditCardId = null
}

const open = ref(props.modelValue)
watch(() => props.modelValue, async (value) => {
  open.value = value

  if (value) {
    reset(blank())

    const [accountList, cardList] = await Promise.allSettled([
      accountsApi.list(),
      creditCardsApi.list()
    ])

    accounts.value = accountList.status === 'fulfilled' ? accountList.value : []
    cards.value = cardList.status === 'fulfilled' ? cardList.value : []
  }
})
watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const purchase = await submit()
  if (!purchase) return

  toast.success(`${purchase.productName} recorded.`)
  emit('saved', purchase)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="t('purchases.recordTitle')">
    <form id="purchase-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.productName"
        :label="t('purchases.productName')"
        :placeholder="t('purchases.productPlaceholder')"
        required
        :error="fieldError('productName')"
      />

      <!-- The method decides which fields below are required -->
      <div class="method">
        <span class="method__label">{{ t('purchases.paidWith') }}</span>

        <div class="method__options" role="radiogroup" :aria-label="t('purchases.paymentMethod')">
          <button
            v-for="option in methodOptions"
            :key="option.value"
            type="button"
            role="radio"
            :aria-checked="values.paymentMethod === option.value"
            :class="['method__option', { 'is-selected': values.paymentMethod === option.value }]"
            @click="chooseMethod(option.value)"
          >
            <BaseIcon :name="option.icon" :size="16" />
            <span class="method__option-title">{{ option.label }}</span>
            <span class="method__option-note">{{ option.note }}</span>
          </button>
        </div>
      </div>

      <BaseSelect
        v-if="isDebit"
        v-model="values.accountId"
        :label="t('common.account')"
        :options="accountOptions"
        :placeholder="accountOptions.length ? t('purchases.chooseAccount') : t('purchases.noAccounts')"
        required
        :error="fieldError('accountId')"
      />

      <BaseSelect
        v-if="isCredit"
        v-model="values.creditCardId"
        :label="t('purchases.card')"
        :options="cardOptions"
        :placeholder="cardOptions.length ? t('purchases.chooseCard') : t('purchases.noCards')"
        required
        :error="fieldError('creditCardId')"
      />

      <BaseSelect
        v-if="isCash"
        v-model="values.currency"
        :label="t('common.currency')"
        :options="CURRENCIES"
        required
        :hint="t('purchases.cashCurrencyHint')"
        :error="fieldError('currency')"
      />

      <div class="form__row">
        <BaseInput
          v-model="values.amount"
          :label="t('common.amount')"
          type="number"
          step="0.01"
          min="0"
          placeholder="0.00"
          required
          :error="fieldError('amount')"
        >
          <template #prefix>{{ effectiveCurrency ?? '' }}</template>
        </BaseInput>

        <BaseInput
          v-model="values.occurredAt"
          :label="t('common.dateAndTime')"
          type="datetime-local"
          :max="now()"
          :error="fieldError('occurredAt')"
        />
      </div>

      <BaseSelect
        v-model="values.category"
        :label="t('common.category')"
        :options="categoryOptions"
        :placeholder="t('common.chooseCategory')"
        required
        :error="fieldError('category')"
      />

      <StorePicker v-model="values.storeId" :error="fieldError('storeId')" />

      <BaseInput
        v-model="values.notes"
        :label="t('common.notes')"
        :placeholder="t('common.optional')"
        :error="fieldError('notes')"
      />
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="purchase-form" variant="primary" :loading="submitting">
        {{ t('purchases.newPurchase') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="@/assets/styles/features/purchases/PurchaseFormModal.scss"></style>
