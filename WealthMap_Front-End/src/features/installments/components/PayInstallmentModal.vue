<script setup>
import { ref, watch, computed } from 'vue'
import { installmentsApi } from '@/api/installments.api'
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
  plan: { type: Object, default: null }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const { format } = useMoney()

const accounts = ref([])

function blank() {
  return { sourceType: PAYMENT_SOURCE.ACCOUNT, sourceAccountId: null, notes: '' }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) =>
  installmentsApi.pay(props.plan.id, {
    sourceType: payload.sourceType,
    sourceAccountId: payload.sourceType === PAYMENT_SOURCE.ACCOUNT ? payload.sourceAccountId : null,
    notes: payload.notes || null
  })
)

/** The endpoint always pays the oldest unpaid row, so the amount is not a choice. */
const nextInstallment = computed(() =>
  props.plan?.payments?.filter((p) => !p.isPaid).sort((a, b) => a.number - b.number)[0] ?? null
)

const open = ref(props.modelValue)
watch(() => props.modelValue, async (value) => {
  open.value = value

  if (value) {
    reset(blank())
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
    result.purchase.isCompleted
      ? `${result.purchase.productName} is fully paid.`
      : `Installment paid — ${result.purchase.remainingMonths} left.`
  )
  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="t('installments.payNextTitle')" size="sm">
    <form id="pay-installment-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <div v-if="nextInstallment && plan" class="next">
        <span class="next__label">
          {{ t('composed.installmentOf', { number: nextInstallment.number, total: plan.monthsCount }) }}
        </span>
        <p class="next__amount numeric">
          {{ format(nextInstallment.amount, { currency: nextInstallment.currency }) }}
        </p>
        <span class="next__due">{{ t('composed.dueOn', { date: nextInstallment.dueDate }) }}</span>
      </div>

      <PaymentSourcePicker
        v-model:source-type="values.sourceType"
        v-model:source-account-id="values.sourceAccountId"
        :accounts="accounts"
        :currency="plan?.currency"
        :error="fieldError('sourceAccountId')"
      />

      <BaseInput
        v-model="values.notes"
        :label="t('common.notes')"
        :placeholder="t('common.optional')"
        :error="fieldError('notes')"
      />
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="pay-installment-form" variant="primary" :loading="submitting">
        {{ t('installments.payInstallment') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="./PayInstallmentModal.scss"></style>
