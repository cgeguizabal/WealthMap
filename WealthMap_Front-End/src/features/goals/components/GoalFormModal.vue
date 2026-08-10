<script setup>
import { ref, watch, computed } from 'vue'
import { savingsGoalsApi, productGoalsApi } from '@/api/goals.api'
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
  goal: { type: Object, default: null },
  kind: { type: String, default: 'savings' }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const auth = useAuthStore()
const { format } = useMoney()

const accounts = ref([])
const isEdit = computed(() => props.goal !== null)
const isSavings = computed(() => props.kind === 'savings')

const CURRENCIES = ['USD', 'MXN', 'EUR', 'GBP', 'CAD', 'BRL', 'COP', 'ARS']
  .map((code) => ({ value: code, label: code }))

function today() {
  return new Date().toISOString().slice(0, 10)
}

function blank() {
  return {
    name: '',
    targetAmount: null,
    currentAmount: null,
    currency: auth.currency,
    deadline: '',
    linkedAccountId: null
  }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) => {
  if (isSavings.value) {
    const body = {
      name: payload.name,
      targetAmount: payload.targetAmount,
      deadline: payload.deadline,
      linkedAccountId: payload.linkedAccountId || null
    }

    return isEdit.value
      ? savingsGoalsApi.update(props.goal.id, body)
      : savingsGoalsApi.create({ ...body, currency: payload.currency, currentAmount: payload.currentAmount ?? 0 })
  }

  const body = {
    name: payload.name,
    targetAmount: payload.targetAmount,
    // Product goals may have no deadline at all — then there is no monthly figure.
    deadline: payload.deadline || null
  }

  return isEdit.value
    ? productGoalsApi.update(props.goal.id, body)
    : productGoalsApi.create({ ...body, currency: payload.currency, currentAmount: payload.currentAmount ?? 0 })
})

/** A goal may only be linked to a savings account in the goal's own currency. */
const linkOptions = computed(() =>
  accounts.value
    .filter((account) => account.type === 'Savings')
    .filter((account) => account.currency === (isEdit.value ? props.goal.currency : values.currency))
    .map((account) => ({
      value: account.id,
      label: `${account.name} — ${format(account.balance, { currency: account.currency })}`
    }))
)

const open = ref(props.modelValue)
watch(() => props.modelValue, async (value) => {
  open.value = value

  if (value) {
    reset(props.goal
      ? {
          ...blank(),
          name: props.goal.name,
          targetAmount: props.goal.targetAmount,
          currency: props.goal.currency,
          deadline: props.goal.deadline ?? '',
          linkedAccountId: props.goal.linkedAccountId ?? null
        }
      : blank())

    if (isSavings.value) {
      try {
        accounts.value = await accountsApi.list()
      } catch {
        accounts.value = []
      }
    }
  }
})
watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const result = await submit()
  if (!result) return

  toast.success(isEdit.value ? 'Goal updated.' : `${result.name} created.`)
  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal
    v-model="open"
    :title="isEdit ? 'Edit goal' : isSavings ? 'New savings goal' : 'New product goal'"
  >
    <form id="goal-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.name"
        label="Name"
        :placeholder="isSavings ? 'Emergency fund' : 'PlayStation 6'"
        required
        :error="fieldError('name')"
      />

      <div class="form__row">
        <BaseInput
          v-model="values.targetAmount"
          label="Target"
          type="number"
          step="0.01"
          min="0"
          required
          :error="fieldError('targetAmount')"
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

      <BaseInput
        v-if="!isEdit"
        v-model="values.currentAmount"
        label="Already saved"
        type="number"
        step="0.01"
        min="0"
        placeholder="0.00"
        hint="Optional — what you have put aside for this already."
        :error="fieldError('currentAmount')"
      />

      <BaseInput
        v-model="values.deadline"
        label="Deadline"
        type="date"
        :min="today()"
        :required="isSavings"
        :hint="isSavings
          ? 'Drives the monthly figure and whether you are on track.'
          : 'Optional. Without one there is no required monthly amount.'"
        :error="fieldError('deadline')"
      />

      <BaseSelect
        v-if="isSavings"
        v-model="values.linkedAccountId"
        label="Linked savings account"
        :options="linkOptions"
        :placeholder="linkOptions.length ? 'None — track only' : 'No savings account in this currency'"
        hint="Link one and contributing moves real money into it."
        :error="fieldError('linkedAccountId')"
      />
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">Cancel</BaseButton>
      <BaseButton type="submit" form="goal-form" variant="primary" :loading="submitting">
        {{ isEdit ? 'Save changes' : 'Create goal' }}
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
