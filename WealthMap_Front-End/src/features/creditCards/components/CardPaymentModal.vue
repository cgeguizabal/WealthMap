<script setup>
import { ref, watch, computed } from 'vue'
import { creditCardsApi } from '@/api/creditCards.api'
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
  card: { type: Object, default: null }
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
  creditCardsApi.pay(props.card.id, {
    amount: payload.amount,
    sourceType: payload.sourceType,
    sourceAccountId: payload.sourceType === PAYMENT_SOURCE.ACCOUNT ? payload.sourceAccountId : null,
    notes: payload.notes || null
  })
)

const owed = computed(() => props.card?.usedCredit ?? 0)

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

function payFull() {
  values.amount = owed.value
}

async function onSubmit() {
  const result = await submit()
  if (!result) return

  toast.success(
    `${format(values.amount, { currency: props.card.currency })} paid — ${format(result.card.usedCredit, { currency: result.card.currency })} still owed.`
  )
  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="t('cards.registerPaymentTitle')">
    <form id="card-payment-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <p v-if="card" class="form__context">
        {{ card.cardName }} · owed
        <strong class="numeric">{{ format(owed, { currency: card.currency }) }}</strong>
      </p>

      <BaseInput
        v-model="values.amount"
        :label="t('common.amount')"
        type="number"
        step="0.01"
        min="0"
        placeholder="0.00"
        required
        :hint="t('cards.paymentAmountHint')"
        :error="fieldError('amount')"
      >
        <template #prefix>{{ card?.currency }}</template>
        <template #suffix>
          <button type="button" class="form__max" @click="payFull">{{ t('cards.payAll') }}</button>
        </template>
      </BaseInput>

      <PaymentSourcePicker
        v-model:source-type="values.sourceType"
        v-model:source-account-id="values.sourceAccountId"
        :accounts="accounts"
        :currency="card?.currency"
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
      <BaseButton type="submit" form="card-payment-form" variant="primary" :loading="submitting">
        {{ t('cards.registerPayment') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="./CardPaymentModal.scss"></style>
