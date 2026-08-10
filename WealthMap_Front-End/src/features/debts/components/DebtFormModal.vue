<script setup>
import { ref, watch, computed } from 'vue'
import { debtsApi } from '@/api/debts.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useAuthStore } from '@/stores/auth.store'

import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  debt: { type: Object, default: null }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const auth = useAuthStore()

const isEdit = computed(() => props.debt !== null)

const CURRENCIES = ['USD', 'MXN', 'EUR', 'GBP', 'CAD', 'BRL', 'COP', 'ARS']
  .map((code) => ({ value: code, label: code }))

const DAYS = Array.from({ length: 31 }, (_, i) => ({ value: i + 1, label: String(i + 1) }))

function blank() {
  return {
    name: '',
    originalAmount: null,
    remainingAmount: null,
    currency: auth.currency,
    monthlyPayment: null,
    monthlyDueDay: 5
  }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) => {
  if (isEdit.value) {
    return debtsApi.update(props.debt.id, {
      name: payload.name,
      monthlyPayment: payload.monthlyPayment,
      monthlyDueDay: payload.monthlyDueDay
    })
  }

  return debtsApi.create({
    name: payload.name,
    originalAmount: payload.originalAmount,
    // Left null means "not paid down at all", which the API reads as the original.
    remainingAmount: payload.remainingAmount ?? null,
    currency: payload.currency,
    monthlyPayment: payload.monthlyPayment,
    monthlyDueDay: payload.monthlyDueDay
  })
})

const open = ref(props.modelValue)
watch(() => props.modelValue, (value) => {
  open.value = value

  if (value) {
    reset(props.debt
      ? {
          ...blank(),
          name: props.debt.name,
          monthlyPayment: props.debt.monthlyPayment,
          monthlyDueDay: props.debt.monthlyDueDay
        }
      : blank())
  }
})
watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const result = await submit()
  if (!result) return

  toast.success(isEdit.value ? 'Debt updated.' : `${result.name} added.`)
  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="isEdit ? 'Edit debt' : 'New debt'">
    <form id="debt-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.name"
        label="Name"
        placeholder="Car loan"
        required
        :error="fieldError('name')"
      />

      <template v-if="!isEdit">
        <div class="form__row">
          <BaseInput
            v-model="values.originalAmount"
            label="Original amount"
            type="number"
            step="0.01"
            min="0"
            required
            :error="fieldError('originalAmount')"
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
          v-model="values.remainingAmount"
          label="Still owed"
          type="number"
          step="0.01"
          min="0"
          placeholder="Same as original"
          hint="Only if you have already paid some of it down."
          :error="fieldError('remainingAmount')"
        />
      </template>

      <div class="form__row">
        <BaseInput
          v-model="values.monthlyPayment"
          label="Monthly payment"
          type="number"
          step="0.01"
          min="0"
          required
          :error="fieldError('monthlyPayment')"
        />

        <BaseSelect
          v-model="values.monthlyDueDay"
          label="Due day"
          :options="DAYS"
          required
          hint="Clamps in short months."
          :error="fieldError('monthlyDueDay')"
        />
      </div>
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">Cancel</BaseButton>
      <BaseButton type="submit" form="debt-form" variant="primary" :loading="submitting">
        {{ isEdit ? 'Save changes' : 'Add debt' }}
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
