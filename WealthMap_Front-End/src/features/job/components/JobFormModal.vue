<script setup>
import { ref, watch, computed } from 'vue'
import { jobsApi, computeNet } from '@/api/jobs.api'
import { accountsApi } from '@/api/accounts.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useMoney } from '@/composables/useMoney'
import { useAuthStore } from '@/stores/auth.store'

import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  job: { type: Object, default: null }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const auth = useAuthStore()
const { format } = useMoney()

const accounts = ref([])
const isEdit = computed(() => props.job !== null)

const CURRENCIES = ['USD', 'MXN', 'EUR', 'GBP', 'CAD', 'BRL', 'COP', 'ARS']
  .map((code) => ({ value: code, label: code }))

const DAYS = Array.from({ length: 31 }, (_, i) => ({ value: i + 1, label: String(i + 1) }))

function blank() {
  return {
    title: '',
    employer: '',
    grossMonthlySalary: null,
    currency: auth.currency,
    depositAccountId: null,
    day1: 15,
    day2: null,
    day3: null
  }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) => {
  // 1–3 distinct days: nulls dropped, duplicates removed.
  const paymentDays = [...new Set([payload.day1, payload.day2, payload.day3].filter(Boolean))]

  const body = {
    title: payload.title,
    employer: payload.employer,
    grossMonthlySalary: payload.grossMonthlySalary,
    depositAccountId: payload.depositAccountId,
    paymentDays
  }

  return isEdit.value
    ? jobsApi.update(props.job.id, body)
    : jobsApi.create({ ...body, currency: payload.currency })
})

const accountOptions = computed(() =>
  accounts.value
    .filter((account) => account.currency === (isEdit.value ? props.job.currency : values.currency))
    .map((account) => ({ value: account.id, label: `${account.name} — ${account.bankName}` }))
)

const dayCount = computed(() =>
  new Set([values.day1, values.day2, values.day3].filter(Boolean)).size
)

/** Net and per-deposit shown live, using the deductions already on the job. */
const preview = computed(() => {
  if (!values.grossMonthlySalary) return null

  const net = computeNet(values.grossMonthlySalary, props.job?.deductions ?? [])
  return { net, perDeposit: dayCount.value ? net / dayCount.value : net }
})

const open = ref(props.modelValue)
watch(() => props.modelValue, async (value) => {
  open.value = value

  if (value) {
    const days = props.job?.paymentDays ?? []

    reset(props.job
      ? {
          ...blank(),
          title: props.job.title,
          employer: props.job.employer,
          grossMonthlySalary: props.job.grossMonthlySalary,
          currency: props.job.currency,
          depositAccountId: props.job.depositAccountId,
          day1: days[0] ?? 15,
          day2: days[1] ?? null,
          day3: days[2] ?? null
        }
      : blank())

    try {
      accounts.value = await accountsApi.list()
    } catch {
      accounts.value = []
    }
  }
})
watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const job = await submit()
  if (!job) return

  toast.success(isEdit.value ? 'Job updated.' : 'Job saved.')
  emit('saved', job)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="isEdit ? t('job.editJob') : t('job.addJob')">
    <form id="job-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <div class="form__row">
        <BaseInput
          v-model="values.title"
          :label="t('job.jobTitle')"
          :placeholder="t('job.jobTitlePlaceholder')"
          required
          :error="fieldError('title')"
        />

        <BaseInput
          v-model="values.employer"
          :label="t('job.employer')"
          :placeholder="t('job.employerPlaceholder')"
          required
          :error="fieldError('employer')"
        />
      </div>

      <div class="form__row">
        <BaseInput
          v-model="values.grossMonthlySalary"
          :label="t('job.grossSalary')"
          type="number"
          step="0.01"
          min="0"
          required
          :hint="t('job.grossHint')"
          :error="fieldError('grossMonthlySalary')"
        />

        <BaseSelect
          v-if="!isEdit"
          v-model="values.currency"
          :label="t('common.currency')"
          :options="CURRENCIES"
          required
          :error="fieldError('currency')"
        />
      </div>

      <BaseSelect
        v-model="values.depositAccountId"
        :label="t('job.paidInto')"
        :options="accountOptions"
        :placeholder="accountOptions.length ? t('job.chooseAccount') : t('job.noAccountInCurrency')"
        required
        :error="fieldError('depositAccountId')"
      />

      <fieldset class="days">
        <legend class="days__legend">{{ t('job.paymentDays') }}</legend>
        <p class="days__hint">
          {{ t('job.paymentDaysHint') }}
        </p>

        <div class="days__row">
          <BaseSelect v-model="values.day1" :label="t('job.first')" :options="DAYS" required />
          <BaseSelect v-model="values.day2" :label="t('job.second')" :options="DAYS" :placeholder="t('job.none')" />
          <BaseSelect v-model="values.day3" :label="t('job.third')" :options="DAYS" :placeholder="t('job.none')" />
        </div>

        <p v-if="fieldError('paymentDays')" class="days__error">
          {{ fieldError('paymentDays')[0] }}
        </p>
      </fieldset>

      <div v-if="preview" class="preview">
        <div class="preview__row">
          <span>{{ t('job.netMonthly') }}</span>
          <span class="numeric">{{ format(preview.net, { currency: values.currency }) }}</span>
        </div>
        <div class="preview__row preview__row--total">
          <span>{{ t('composed.perDepositTimes', { count: dayCount }) }}</span>
          <span class="numeric">{{ format(preview.perDeposit, { currency: values.currency }) }}</span>
        </div>
        <p v-if="!isEdit" class="preview__note">{{ t('job.deductionsAfterSave') }}</p>
      </div>
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="job-form" variant="primary" :loading="submitting">
        {{ isEdit ? t('common.saveChanges') : t('job.saveJob') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="./JobFormModal.scss"></style>
