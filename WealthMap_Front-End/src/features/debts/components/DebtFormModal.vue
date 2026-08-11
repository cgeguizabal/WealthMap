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
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

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
  <BaseModal v-model="open" :title="isEdit ? t('debts.editDebt') : t('debts.newDebt')">
    <form id="debt-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.name"
        :label="t('common.name')"
        :placeholder="t('debts.namePlaceholder')"
        required
        :error="fieldError('name')"
      />

      <template v-if="!isEdit">
        <div class="form__row">
          <BaseInput
            v-model="values.originalAmount"
            :label="t('debts.originalAmount')"
            type="number"
            step="0.01"
            min="0"
            required
            :error="fieldError('originalAmount')"
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
          v-model="values.remainingAmount"
          :label="t('debts.stillOwed')"
          type="number"
          step="0.01"
          min="0"
          :placeholder="t('debts.sameAsOriginal')"
          :hint="t('debts.stillOwedHint')"
          :error="fieldError('remainingAmount')"
        />
      </template>

      <div class="form__row">
        <BaseInput
          v-model="values.monthlyPayment"
          :label="t('debts.monthlyPayment')"
          type="number"
          step="0.01"
          min="0"
          required
          :error="fieldError('monthlyPayment')"
        />

        <BaseSelect
          v-model="values.monthlyDueDay"
          :label="t('debts.dueDay')"
          :options="DAYS"
          required
          :hint="t('debts.clampsHint')"
          :error="fieldError('monthlyDueDay')"
        />
      </div>
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="debt-form" variant="primary" :loading="submitting">
        {{ isEdit ? t('common.saveChanges') : t('debts.addDebt') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="@/assets/styles/features/debts/DebtFormModal.scss"></style>
