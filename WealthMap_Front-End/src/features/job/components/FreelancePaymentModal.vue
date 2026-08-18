<script setup>
import { ref, watch, computed, onMounted } from 'vue'
import { freelanceJobsApi } from '@/api/freelanceJobs.api'
import { accountsApi } from '@/api/accounts.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useMoney } from '@/composables/useMoney'
import { useDashboardStore } from '@/stores/dashboard.store'

import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'

/**
 * Records that a client paid, on whatever date they actually paid.
 *
 * This is the only screen in the freelance flow that moves money: the amount
 * lands in the chosen account as a deposit, which is why it asks which account
 * rather than assuming one. From that moment the money is ordinary balance and
 * counts toward what is safe to spend, exactly like a salary deposit.
 */
const { t } = useI18n()

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  job: { type: Object, default: null }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const { format } = useMoney()
const dashboard = useDashboardStore()

const accounts = ref([])

/**
 * Only accounts that can hold this money. Offering one in another currency would
 * produce a server error the user could not have predicted from the list.
 */
const accountOptions = computed(() =>
  accounts.value
    .filter((account) => account.currency === props.job?.currency)
    .map((account) => ({
      value: account.id,
      label: `${account.name} · ${format(account.balance, { currency: account.currency })}`
    }))
)

const noEligibleAccount = computed(() => props.job !== null && accountOptions.value.length === 0)

function blank() {
  return {
    amountPaid: null,
    depositAccountId: '',
    // Today, because recording a payment usually happens the day it arrives —
    // and it stays editable for the times it does not.
    paidOn: new Date().toISOString().slice(0, 10)
  }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) =>
  freelanceJobsApi.markPaid(props.job.id, {
    amountPaid: payload.amountPaid,
    depositAccountId: payload.depositAccountId,
    paidOn: payload.paidOn
  })
)

const open = ref(props.modelValue)

watch(() => props.modelValue, async (value) => {
  open.value = value
  if (!value) return

  reset({
    ...blank(),
    // Prefilled with what was agreed, since that is what usually arrives. The
    // field stays editable for the times a client pays short or adds a bonus.
    amountPaid: props.job?.agreedAmount ?? null
  })

  if (accounts.value.length === 0) accounts.value = await accountsApi.list()

  const [first] = accountOptions.value
  if (first) values.depositAccountId = first.value
})

watch(open, (value) => emit('update:modelValue', value))

onMounted(async () => {
  if (accounts.value.length === 0) accounts.value = await accountsApi.list()
})

async function onSubmit() {
  const result = await submit()
  if (!result) return

  toast.success(t('freelance.paymentRecorded', {
    amount: format(result.amountPaid, { currency: result.currency })
  }))

  // The deposit changed a balance, so every figure on the dashboard is now stale.
  dashboard.load()

  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="t('freelance.recordPayment')">
    <form id="freelance-payment-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <p v-if="job" class="form__note">
        {{ t('freelance.agreedWas', {
          amount: format(job.agreedAmount, { currency: job.currency })
        }) }}
      </p>

      <p v-if="noEligibleAccount" class="form__error" role="alert">
        {{ t('freelance.noAccountInCurrency', { currency: job?.currency }) }}
      </p>

      <BaseInput
        v-model="values.amountPaid"
        :label="t('freelance.amountReceived')"
        type="number"
        step="0.01"
        min="0"
        required
        :hint="t('freelance.amountReceivedHint')"
        :error="fieldError('amountPaid')"
      />

      <BaseSelect
        v-model="values.depositAccountId"
        :label="t('freelance.depositTo')"
        :options="accountOptions"
        required
        :hint="t('freelance.depositToHint')"
        :error="fieldError('depositAccountId')"
      />

      <BaseInput
        v-model="values.paidOn"
        :label="t('freelance.paidOn')"
        type="date"
        required
        :error="fieldError('paidOn')"
      />
    </form>

    <template #footer>
      <BaseButton variant="ghost" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton
        type="submit"
        form="freelance-payment-form"
        variant="primary"
        :loading="submitting"
        :disabled="noEligibleAccount"
      >
        {{ t('freelance.confirmPayment') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="@/assets/styles/features/job/JobFormModal.scss"></style>
