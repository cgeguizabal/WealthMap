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
import { useI18n } from '@/composables/useI18n'
import { CURRENCY_OPTIONS as CURRENCIES } from '@/config/currencies'

const { t } = useI18n()

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
    :title="isEdit ? t('goals.editGoal') : isSavings ? t('goals.newSavingsGoal') : t('goals.newProductGoal')"
  >
    <form id="goal-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.name"
        :label="t('common.name')"
        :placeholder="isSavings ? t('goals.savingsPlaceholder') : t('goals.productPlaceholder')"
        required
        :error="fieldError('name')"
      />

      <div class="form__row">
        <BaseInput
          v-model="values.targetAmount"
          :label="t('goals.target')"
          type="number"
          step="0.01"
          min="0"
          required
          :error="fieldError('targetAmount')"
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

      <BaseInput
        v-if="!isEdit"
        v-model="values.currentAmount"
        :label="t('goals.alreadySaved')"
        type="number"
        step="0.01"
        min="0"
        placeholder="0.00"
        :hint="t('goals.alreadySavedHint')"
        :error="fieldError('currentAmount')"
      />

      <BaseInput
        v-model="values.deadline"
        :label="t('goals.deadline')"
        type="date"
        :min="today()"
        :required="isSavings"
        :hint="isSavings ? t('goals.savingsDeadlineHint') : t('goals.productDeadlineHint')"
        :error="fieldError('deadline')"
      />

      <BaseSelect
        v-if="isSavings"
        v-model="values.linkedAccountId"
        :label="t('goals.linkedAccount')"
        :options="linkOptions"
        :placeholder="linkOptions.length ? t('goals.trackOnly') : t('goals.noSavingsAccount')"
        :hint="t('goals.linkHint')"
        :error="fieldError('linkedAccountId')"
      />
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="goal-form" variant="primary" :loading="submitting">
        {{ isEdit ? t('common.saveChanges') : t('goals.createGoal') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="@/assets/styles/features/goals/GoalFormModal.scss"></style>
