<script setup>
import { ref, watch, computed } from 'vue'
import { creditCardsApi } from '@/api/creditCards.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useAuthStore } from '@/stores/auth.store'
import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import TrackingFields from '@/features/shared/components/TrackingFields.vue'
import { TRACKING_MODE, trackingModeValue } from '@/api/tracking'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  card: { type: Object, default: null }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const auth = useAuthStore()

const isEdit = computed(() => props.card !== null)

const CURRENCIES = ['USD', 'MXN', 'EUR', 'GBP', 'CAD', 'BRL', 'COP', 'ARS']
  .map((code) => ({ value: code, label: code }))

const DAYS = Array.from({ length: 31 }, (_, i) => ({ value: i + 1, label: String(i + 1) }))

function blank() {
  return {
    cardName: '',
    bankName: '',
    creditLimit: null,
    currency: auth.currency,
    annualInterestRate: null,
    paymentDueDay: 15,
    statementCutoffDay: 28,
    lastFour: '',
    trackingMode: TRACKING_MODE.MANUAL,
    notes: ''
  }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), async (payload) => {
  const tracking = {
    trackingMode: payload.trackingMode,
    lastFour: payload.lastFour || null
  }

  if (isEdit.value) {
    // The limit has its own endpoint; used credit only moves through charges and payments.
    const updated = await creditCardsApi.update(props.card.id, {
      cardName: payload.cardName,
      bankName: payload.bankName,
      annualInterestRate: payload.annualInterestRate ?? 0,
      paymentDueDay: payload.paymentDueDay,
      statementCutoffDay: payload.statementCutoffDay,
      notes: payload.notes || null
    })

    // A second call only when something actually changed. Tracking has its own
    // endpoint because the two fields constrain each other, and sending an
    // unchanged pair on every save would be a write for nothing.
    const changed =
      tracking.lastFour !== (props.card.lastFour ?? null) ||
      tracking.trackingMode !== trackingModeValue(props.card.trackingMode)

    return changed ? creditCardsApi.updateTracking(props.card.id, tracking) : updated
  }

  return creditCardsApi.create({
    cardName: payload.cardName,
    bankName: payload.bankName,
    creditLimit: payload.creditLimit,
    currency: payload.currency,
    annualInterestRate: payload.annualInterestRate ?? 0,
    paymentDueDay: payload.paymentDueDay,
    statementCutoffDay: payload.statementCutoffDay,
    ...tracking
  })
})

const open = ref(props.modelValue)
watch(() => props.modelValue, (value) => {
  open.value = value

  if (value) {
    reset(props.card
      ? {
          ...blank(),
          cardName: props.card.cardName,
          bankName: props.card.bankName,
          annualInterestRate: props.card.annualInterestRate,
          paymentDueDay: props.card.paymentDueDay,
          statementCutoffDay: props.card.statementCutoffDay,
          lastFour: props.card.lastFour ?? '',
          trackingMode: trackingModeValue(props.card.trackingMode),
          notes: props.card.notes ?? ''
        }
      : blank())
  }
})
watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const result = await submit()
  if (!result) return

  toast.success(isEdit.value ? 'Card updated.' : `${result.cardName} added.`)
  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="isEdit ? t('cards.editCard') : t('cards.newCard')">
    <form id="card-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.cardName"
        :label="t('cards.cardName')"
        :placeholder="t('cards.cardNamePlaceholder')"
        required
        :error="fieldError('cardName')"
      />

      <BaseInput
        v-model="values.bankName"
        :label="t('accounts.bankName')"
        :placeholder="t('accounts.bankPlaceholder')"
        required
        :error="fieldError('bankName')"
      />

      <div v-if="!isEdit" class="form__row">
        <BaseInput
          v-model="values.creditLimit"
          :label="t('cards.creditLimit')"
          type="number"
          step="0.01"
          min="0"
          placeholder="5000.00"
          required
          :error="fieldError('creditLimit')"
        />

        <BaseSelect
          v-model="values.currency"
          :label="t('common.currency')"
          :options="CURRENCIES"
          required
          :error="fieldError('currency')"
        />
      </div>

      <BaseInput
        v-model="values.annualInterestRate"
        :label="t('cards.interestRate')"
        type="number"
        step="0.001"
        min="0"
        max="200"
        placeholder="45.9"
        :error="fieldError('annualInterestRate')"
      >
        <template #suffix>%</template>
      </BaseInput>

      <div class="form__row">
        <BaseSelect
          v-model="values.paymentDueDay"
          :label="t('cards.paymentDueDay')"
          :options="DAYS"
          required
          :hint="t('cards.clampsHint')"
          :error="fieldError('paymentDueDay')"
        />

        <BaseSelect
          v-model="values.statementCutoffDay"
          :label="t('cards.statementCutoff')"
          :options="DAYS"
          required
          :error="fieldError('statementCutoffDay')"
        />
      </div>

      <TrackingFields
        v-model:last-four="values.lastFour"
        v-model:tracking-mode="values.trackingMode"
        :error="fieldError('lastFour')"
      />

      <BaseInput
        v-if="isEdit"
        v-model="values.notes"
        :label="t('common.notes')"
        :placeholder="t('common.optional')"
        :error="fieldError('notes')"
      />
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="card-form" variant="primary" :loading="submitting">
        {{ isEdit ? t('common.saveChanges') : t('cards.addCard') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="@/assets/styles/features/creditCards/CardFormModal.scss"></style>
