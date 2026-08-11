<script setup>
import { ref, watch, computed } from 'vue'
import { incomesApi, INCOME_FREQUENCY_OPTIONS, toMonthly } from '@/api/incomes.api'
import { useServerText } from '@/composables/useServerText'
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
const { label: serverLabel } = useServerText()

/** Values and names stay as the API knows them; only the wording is localised. */
const frequencyOptions = computed(() =>
  INCOME_FREQUENCY_OPTIONS.map((o) => ({
    ...o,
    label: serverLabel('incomeFrequency', o.name)
  }))
)

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  income: { type: Object, default: null }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const auth = useAuthStore()
const { format } = useMoney()

const accounts = ref([])
const isEdit = computed(() => props.income !== null)

const CURRENCIES = ['USD', 'MXN', 'EUR', 'GBP', 'CAD', 'BRL', 'COP', 'ARS']
  .map((code) => ({ value: code, label: code }))

function blank() {
  return { name: '', amount: null, currency: auth.currency, frequency: 3, depositAccountId: null }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) => {
  const body = {
    name: payload.name,
    amount: payload.amount,
    currency: payload.currency,
    frequency: payload.frequency,
    depositAccountId: payload.depositAccountId
  }

  return isEdit.value ? incomesApi.update(props.income.id, body) : incomesApi.create(body)
})

const accountOptions = computed(() =>
  accounts.value.map((account) => ({
    value: account.id,
    label: `${account.name} — ${account.bankName}`
  }))
)

/** Frequencies are normalised to a monthly figure on the dashboard, so show it here too. */
const monthlyEquivalent = computed(() => {
  if (!values.amount) return null

  // The enum name, not the label: toMonthly switches on 'Weekly'/'Biweekly'.
  const name = INCOME_FREQUENCY_OPTIONS.find((o) => o.value === values.frequency)?.name
  return toMonthly(values.amount, name)
})

const open = ref(props.modelValue)
watch(() => props.modelValue, async (value) => {
  open.value = value

  if (value) {
    reset(props.income
      ? {
          name: props.income.name,
          amount: props.income.amount,
          currency: props.income.currency,
          frequency: INCOME_FREQUENCY_OPTIONS.find((o) => o.name === props.income.frequency)?.value ?? 3,
          depositAccountId: props.income.depositAccountId
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
  const result = await submit()
  if (!result) return

  toast.success(isEdit.value ? 'Income updated.' : `${result.name} added.`)
  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="isEdit ? t('job.editIncome') : t('job.addRecurringIncome')" size="sm">
    <form id="income-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.name"
        :label="t('common.name')"
        :placeholder="t('job.incomePlaceholder')"
        required
        :error="fieldError('name')"
      />

      <div class="form__row">
        <BaseInput
          v-model="values.amount"
          :label="t('common.amount')"
          type="number"
          step="0.01"
          min="0"
          required
          :error="fieldError('amount')"
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
        v-model="values.frequency"
        :label="t('job.frequency')"
        :options="frequencyOptions"
        required
        :error="fieldError('frequency')"
      />

      <BaseSelect
        v-model="values.depositAccountId"
        :label="t('job.paidInto')"
        :options="accountOptions"
        :placeholder="accountOptions.length ? t('job.chooseAccount') : t('job.noAccounts')"
        required
        :error="fieldError('depositAccountId')"
      />

      <p v-if="monthlyEquivalent !== null" class="equivalent">
        {{ t('job.countsAs') }}
        <strong class="numeric">{{ format(monthlyEquivalent, { currency: values.currency }) }}</strong>
        {{ t('job.perMonthInTotals') }}
      </p>

      <p class="note">
        {{ t('job.incomeHint') }}
      </p>
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="income-form" variant="primary" :loading="submitting">
        {{ isEdit ? t('common.saveChanges') : t('job.addIncome') }}
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

.equivalent {
  padding: var(--sp-3);
  background: var(--canvas-alt);
  border-radius: var(--radius-sm);
  font-size: var(--fs-sm);
  color: var(--text-muted);

  strong { color: var(--text); font-weight: var(--fw-semibold); }
}

.note { font-size: var(--fs-xs); color: var(--text-subtle); line-height: 1.5; }

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
