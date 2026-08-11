<script setup>
import { ref, watch, computed } from 'vue'
import { debtsApi } from '@/api/debts.api'
import { accountsApi } from '@/api/accounts.api'
import { PAYMENT_SOURCE } from '@/api/payments.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useMoney } from '@/composables/useMoney'

import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import PaymentSourcePicker from '@/features/shared/components/PaymentSourcePicker.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  debt: { type: Object, default: null }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const { format } = useMoney()

const accounts = ref([])

function blank() {
  return {
    amount: null,
    sourceType: PAYMENT_SOURCE.ACCOUNT,
    sourceAccountId: null,
    notes: ''
  }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) =>
  debtsApi.pay(props.debt.id, {
    amount: payload.amount,
    sourceType: payload.sourceType,
    sourceAccountId: payload.sourceType === PAYMENT_SOURCE.ACCOUNT ? payload.sourceAccountId : null,
    notes: payload.notes || null
  })
)

const remaining = computed(() => props.debt?.remainingAmount ?? 0)

const open = ref(props.modelValue)
watch(() => props.modelValue, async (value) => {
  open.value = value

  if (value) {
    // Pre-filling the scheduled payment covers the common case in one click.
    reset({ ...blank(), amount: props.debt?.monthlyPayment ?? null })

    try {
      accounts.value = await accountsApi.list()
    } catch {
      accounts.value = []
    }
  }
})
watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const result = await submit()
  if (!result) return

  toast.success(
    result.debt.status === 'PaidOff'
      ? `${result.debt.name} is paid off.`
      : `Paid — ${format(result.debt.remainingAmount, { currency: result.debt.currency })} still owed.`
  )
  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="t('debts.registerPaymentTitle')" size="sm">
    <form id="debt-payment-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <p v-if="debt" class="form__context">
        {{ debt.name }} · remaining
        <strong class="numeric">{{ format(remaining, { currency: debt.currency }) }}</strong>
      </p>

      <BaseInput
        v-model="values.amount"
        :label="t('common.amount')"
        type="number"
        step="0.01"
        min="0"
        required
        :hint="t('debts.paymentHint')"
        :error="fieldError('amount')"
      >
        <template #prefix>{{ debt?.currency }}</template>
        <template #suffix>
          <button type="button" class="form__max" @click="values.amount = remaining">{{ t('debts.payAll') }}</button>
        </template>
      </BaseInput>

      <PaymentSourcePicker
        v-model:source-type="values.sourceType"
        v-model:source-account-id="values.sourceAccountId"
        :accounts="accounts"
        :currency="debt?.currency"
        :error="fieldError('sourceAccountId')"
      />

      <BaseInput
        v-model="values.notes"
        :label="t('common.notes')"
        :placeholder="t('common.optional')"
        :error="fieldError('notes')"
      />

      <p v-if="debt?.status === 'Defaulted'" class="form__hint">
        {{ t('debts.defaultedHint') }}
      </p>
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="debt-payment-form" variant="primary" :loading="submitting">
        {{ t('debts.registerPayment') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="@/assets/styles/features/debts/DebtPaymentModal.scss"></style>
