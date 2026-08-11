<script setup>
import { ref, watch, computed } from 'vue'
import { savingsGoalsApi, productGoalsApi } from '@/api/goals.api'
import { accountsApi } from '@/api/accounts.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useMoney } from '@/composables/useMoney'

import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  goal: { type: Object, default: null },
  kind: { type: String, default: 'savings' }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const { format } = useMoney()

const accounts = ref([])

/** Only a savings goal with a linked account moves real money. */
const isLinked = computed(() => props.kind === 'savings' && Boolean(props.goal?.linkedAccountId))

const remaining = computed(() => {
  if (!props.goal) return 0
  return Math.max(0, props.goal.targetAmount - props.goal.currentAmount)
})

/** The linked account is the destination, so it cannot also be the source. */
const sourceOptions = computed(() =>
  accounts.value
    .filter((account) => account.id !== props.goal?.linkedAccountId)
    .filter((account) => account.currency === props.goal?.currency)
    .map((account) => ({
      value: account.id,
      label: `${account.name} — ${format(account.balance, { currency: account.currency })}${account.isBlockedForSaving ? ' (blocked)' : ''}`,
      disabled: account.isBlockedForSaving
    }))
)

const { values, submitting, formError, submit, reset, fieldError } = useForm(
  { amount: null, sourceAccountId: null },
  (payload) => {
    if (props.kind === 'product') {
      return productGoalsApi.contribute(props.goal.id, payload.amount)
    }

    return savingsGoalsApi.contribute(props.goal.id, {
      amount: payload.amount,
      // Sending an account on an unlinked goal is rejected outright.
      sourceAccountId: isLinked.value ? payload.sourceAccountId : null
    })
  }
)

const open = ref(props.modelValue)
watch(() => props.modelValue, async (value) => {
  open.value = value

  if (value) {
    reset({ amount: null, sourceAccountId: null })

    if (isLinked.value) {
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

  // Savings contributions answer with a wrapper; product goals return the goal.
  const goal = props.kind === 'product' ? result : result.goal

  toast.success(
    goal.status === 'Completed'
      ? `${goal.name} is fully funded.`
      : `Added — ${format(goal.currentAmount, { currency: goal.currency })} saved so far.`
  )

  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="t('goals.contributeTitle')" size="sm">
    <form id="contribute-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <div v-if="goal" class="context">
        <span class="context__name">{{ goal.name }}</span>
        <p class="context__amounts numeric">
          {{ format(goal.currentAmount, { currency: goal.currency }) }}
          of {{ format(goal.targetAmount, { currency: goal.currency }) }}
        </p>
        <span class="context__left numeric">
          {{ format(remaining, { currency: goal.currency }) }} to go
        </span>
      </div>

      <BaseInput
        v-model="values.amount"
        :label="t('common.amount')"
        type="number"
        step="0.01"
        min="0"
        required
        :error="fieldError('amount')"
      >
        <template #prefix>{{ goal?.currency }}</template>
        <template #suffix>
          <button type="button" class="form__max" @click="values.amount = remaining">
            {{ t('goals.fillIt') }}
          </button>
        </template>
      </BaseInput>

      <template v-if="isLinked">
        <BaseSelect
          v-model="values.sourceAccountId"
          :label="t('goals.moveFrom')"
          :options="sourceOptions"
          :placeholder="sourceOptions.length ? t('goals.chooseAccount') : t('goals.noEligibleAccounts')"
          required
          :error="fieldError('sourceAccountId')"
        />

        <p class="form__note">
          <BaseIcon name="transfer" :size="14" />
          {{ t('goals.contributeHint') }}
        </p>
      </template>

      <p v-else class="form__note">
        <BaseIcon name="info" :size="14" />
        {{ kind === 'product' ? t('goals.productNoMoney') : t('goals.noLinkedAccount') }}
      </p>
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="contribute-form" variant="primary" :loading="submitting">
        {{ t('goals.contribute') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss">
.form { display: flex; flex-direction: column; gap: var(--sp-4); }

.context {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: var(--sp-3) var(--sp-4);
  background: var(--canvas-alt);
  border-radius: var(--radius-sm);
}

.context__name { font-size: var(--fs-sm); font-weight: var(--fw-semibold); }
.context__amounts { font-size: var(--fs-sm); color: var(--text-muted); }
.context__left { font-size: var(--fs-xs); color: var(--text-subtle); }

.form__max {
  border: none;
  background: transparent;
  color: var(--accent);
  font-size: var(--fs-xs);
  font-weight: var(--fw-semibold);
  cursor: pointer;
  padding: 0;

  &:hover { text-decoration: underline; }
}

.form__note {
  display: flex;
  align-items: flex-start;
  gap: var(--sp-2);
  font-size: var(--fs-xs);
  color: var(--text-muted);
  line-height: 1.5;
}

.form__error {
  padding: var(--sp-3);
  border: 1px solid var(--negative);
  border-radius: var(--radius);
  background: var(--negative-soft);
  color: var(--negative);
  font-size: var(--fs-sm);
}
</style>
