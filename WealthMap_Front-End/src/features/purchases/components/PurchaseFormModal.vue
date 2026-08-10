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
import { useAuthStore } from '@/stores/auth.store'

import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import StorePicker from '@/features/shared/components/StorePicker.vue'

const props = defineProps({
  modelValue: { type: Boolean, default: false }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const { format } = useMoney()
const auth = useAuthStore()

const accounts = ref([])
const cards = ref([])

const CURRENCIES = ['USD', 'MXN', 'EUR', 'GBP', 'CAD', 'BRL', 'COP', 'ARS']
  .map((code) => ({ value: code, label: code }))

const categoryOptions = PURCHASE_CATEGORIES.map((name) => ({ value: name, label: name }))

function today() {
  return new Date().toISOString().slice(0, 10)
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
    occurredAt: today(),
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
    occurredAt: payload.occurredAt || null,
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
  <BaseModal v-model="open" title="Record a purchase">
    <form id="purchase-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.productName"
        label="What did you buy?"
        placeholder="Groceries"
        required
        :error="fieldError('productName')"
      />

      <!-- The method decides which fields below are required -->
      <div class="method">
        <span class="method__label">Paid with</span>

        <div class="method__options" role="radiogroup" aria-label="Payment method">
          <button
            v-for="option in PAYMENT_METHOD_OPTIONS"
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
        label="Account"
        :options="accountOptions"
        :placeholder="accountOptions.length ? 'Choose an account' : 'No accounts yet'"
        required
        :error="fieldError('accountId')"
      />

      <BaseSelect
        v-if="isCredit"
        v-model="values.creditCardId"
        label="Card"
        :options="cardOptions"
        :placeholder="cardOptions.length ? 'Choose a card' : 'No cards yet'"
        required
        :error="fieldError('creditCardId')"
      />

      <BaseSelect
        v-if="isCash"
        v-model="values.currency"
        label="Currency"
        :options="CURRENCIES"
        required
        hint="Cash has no account to inherit a currency from."
        :error="fieldError('currency')"
      />

      <div class="form__row">
        <BaseInput
          v-model="values.amount"
          label="Amount"
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
          label="Date"
          type="date"
          :max="today()"
          :error="fieldError('occurredAt')"
        />
      </div>

      <BaseSelect
        v-model="values.category"
        label="Category"
        :options="categoryOptions"
        placeholder="Choose a category"
        required
        :error="fieldError('category')"
      />

      <StorePicker v-model="values.storeId" :error="fieldError('storeId')" />

      <BaseInput
        v-model="values.notes"
        label="Notes"
        placeholder="Optional"
        :error="fieldError('notes')"
      />
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">Cancel</BaseButton>
      <BaseButton type="submit" form="purchase-form" variant="primary" :loading="submitting">
        Record purchase
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss">
.form { display: flex; flex-direction: column; gap: var(--sp-4); }

.form__row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--sp-4);
}

.method { display: flex; flex-direction: column; gap: var(--sp-2); }
.method__label { font-size: var(--fs-sm); font-weight: var(--fw-medium); }

.method__options {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--sp-2);
}

.method__option {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 2px;

  padding: var(--sp-3);
  border: var(--border);
  border-radius: var(--radius);
  background: var(--surface);
  text-align: left;
  cursor: pointer;

  @include focus-ring;
  transition: background var(--dur-fast) var(--ease);

  &:hover { background: var(--canvas-alt); }

  &.is-selected {
    background: var(--canvas-alt);
    border-color: var(--accent);
    box-shadow: var(--shadow-sm);
  }
}

.method__option-title {
  font-size: var(--fs-sm);
  font-weight: var(--fw-semibold);
  margin-top: var(--sp-1);
}

.method__option-note { font-size: var(--fs-xs); color: var(--text-muted); line-height: 1.35; }

.form__error {
  padding: var(--sp-3);
  border: 1px solid var(--negative);
  border-radius: var(--radius);
  background: var(--negative-soft);
  color: var(--negative);
  font-size: var(--fs-sm);
}

@media (max-width: 560px) {
  .form__row, .method__options { grid-template-columns: 1fr; }
}
</style>
