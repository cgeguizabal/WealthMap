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
    notes: ''
  }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) => {
  if (isEdit.value) {
    // The limit has its own endpoint; used credit only moves through charges and payments.
    return creditCardsApi.update(props.card.id, {
      cardName: payload.cardName,
      bankName: payload.bankName,
      annualInterestRate: payload.annualInterestRate ?? 0,
      paymentDueDay: payload.paymentDueDay,
      statementCutoffDay: payload.statementCutoffDay,
      notes: payload.notes || null
    })
  }

  return creditCardsApi.create({
    cardName: payload.cardName,
    bankName: payload.bankName,
    creditLimit: payload.creditLimit,
    currency: payload.currency,
    annualInterestRate: payload.annualInterestRate ?? 0,
    paymentDueDay: payload.paymentDueDay,
    statementCutoffDay: payload.statementCutoffDay
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
  <BaseModal v-model="open" :title="isEdit ? 'Edit card' : 'New credit card'">
    <form id="card-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.cardName"
        label="Card name"
        placeholder="Gold"
        required
        :error="fieldError('cardName')"
      />

      <BaseInput
        v-model="values.bankName"
        label="Bank"
        placeholder="BBVA"
        required
        :error="fieldError('bankName')"
      />

      <div v-if="!isEdit" class="form__row">
        <BaseInput
          v-model="values.creditLimit"
          label="Credit limit"
          type="number"
          step="0.01"
          min="0"
          placeholder="5000.00"
          required
          :error="fieldError('creditLimit')"
        />

        <BaseSelect
          v-model="values.currency"
          label="Currency"
          :options="CURRENCIES"
          required
          :error="fieldError('currency')"
        />
      </div>

      <BaseInput
        v-model="values.annualInterestRate"
        label="Annual interest rate"
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
          label="Payment due day"
          :options="DAYS"
          required
          hint="Clamps in short months."
          :error="fieldError('paymentDueDay')"
        />

        <BaseSelect
          v-model="values.statementCutoffDay"
          label="Statement cutoff"
          :options="DAYS"
          required
          :error="fieldError('statementCutoffDay')"
        />
      </div>

      <BaseInput
        v-if="isEdit"
        v-model="values.notes"
        label="Notes"
        placeholder="Optional"
        :error="fieldError('notes')"
      />
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">Cancel</BaseButton>
      <BaseButton type="submit" form="card-form" variant="primary" :loading="submitting">
        {{ isEdit ? 'Save changes' : 'Add card' }}
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

.form__error {
  padding: var(--sp-3);
  border: 1px solid var(--negative);
  border-radius: var(--radius);
  background: var(--negative-soft);
  color: var(--negative);
  font-size: var(--fs-sm);
}

@media (max-width: 480px) {
  .form__row { grid-template-columns: 1fr; }
}
</style>
