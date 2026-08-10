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
  <BaseModal v-model="open" :title="isEdit ? 'Edit job' : 'Add your job'">
    <form id="job-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <div class="form__row">
        <BaseInput
          v-model="values.title"
          label="Job title"
          placeholder="Full-stack developer"
          required
          :error="fieldError('title')"
        />

        <BaseInput
          v-model="values.employer"
          label="Employer"
          placeholder="Acme"
          required
          :error="fieldError('employer')"
        />
      </div>

      <div class="form__row">
        <BaseInput
          v-model="values.grossMonthlySalary"
          label="Gross monthly salary"
          type="number"
          step="0.01"
          min="0"
          required
          hint="Before deductions."
          :error="fieldError('grossMonthlySalary')"
        />

        <BaseSelect
          v-if="!isEdit"
          v-model="values.currency"
          label="Currency"
          :options="CURRENCIES"
          required
          :error="fieldError('currency')"
        />
      </div>

      <BaseSelect
        v-model="values.depositAccountId"
        label="Paid into"
        :options="accountOptions"
        :placeholder="accountOptions.length ? 'Choose an account' : 'No account in this currency'"
        required
        :error="fieldError('depositAccountId')"
      />

      <fieldset class="days">
        <legend class="days__legend">Payment days</legend>
        <p class="days__hint">
          Between one and three days a month. A day past the month's end clamps to the last day.
        </p>

        <div class="days__row">
          <BaseSelect v-model="values.day1" label="First" :options="DAYS" required />
          <BaseSelect v-model="values.day2" label="Second" :options="DAYS" placeholder="None" />
          <BaseSelect v-model="values.day3" label="Third" :options="DAYS" placeholder="None" />
        </div>

        <p v-if="fieldError('paymentDays')" class="days__error">
          {{ fieldError('paymentDays')[0] }}
        </p>
      </fieldset>

      <div v-if="preview" class="preview">
        <div class="preview__row">
          <span>Net monthly</span>
          <span class="numeric">{{ format(preview.net, { currency: values.currency }) }}</span>
        </div>
        <div class="preview__row preview__row--total">
          <span>Per deposit ({{ dayCount }}×)</span>
          <span class="numeric">{{ format(preview.perDeposit, { currency: values.currency }) }}</span>
        </div>
        <p v-if="!isEdit" class="preview__note">Deductions are added after the job is saved.</p>
      </div>
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">Cancel</BaseButton>
      <BaseButton type="submit" form="job-form" variant="primary" :loading="submitting">
        {{ isEdit ? 'Save changes' : 'Save job' }}
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

.days { border: none; padding: 0; margin: 0; }

.days__legend {
  padding: 0;
  font-size: var(--fs-sm);
  font-weight: var(--fw-medium);
}

.days__hint {
  margin: var(--sp-1) 0 var(--sp-3);
  font-size: var(--fs-xs);
  color: var(--text-muted);
}

.days__row {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--sp-3);
}

.days__error { margin-top: var(--sp-2); font-size: var(--fs-xs); color: var(--negative); }

.preview {
  display: flex;
  flex-direction: column;
  gap: var(--sp-2);
  padding: var(--sp-3) var(--sp-4);
  background: var(--canvas-alt);
  border: var(--border-subtle);
  border-radius: var(--radius);
}

.preview__row {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: var(--sp-3);
  font-size: var(--fs-sm);
  color: var(--text-muted);
}

.preview__row--total {
  padding-top: var(--sp-2);
  border-top: var(--border-subtle);
  color: var(--text);
  font-weight: var(--fw-semibold);
}

.preview__note { font-size: var(--fs-xs); color: var(--text-subtle); }

.form__error {
  padding: var(--sp-3);
  border: 1px solid var(--negative);
  border-radius: var(--radius);
  background: var(--negative-soft);
  color: var(--negative);
  font-size: var(--fs-sm);
}

@media (max-width: 560px) {
  .form__row, .days__row { grid-template-columns: 1fr; }
}
</style>
