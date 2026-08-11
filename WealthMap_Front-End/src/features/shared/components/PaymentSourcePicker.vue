<script setup>
import { computed, watch } from 'vue'
import { useMoney } from '@/composables/useMoney'
import { PAYMENT_SOURCE } from '@/api/payments.api'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

/**
 * The Account/External choice is identical for cards, debts and installments,
 * so it lives here once. External means cash or a third party paid: the balance
 * falls, no account is touched, and no movement is written — only a payment row.
 */
const props = defineProps({
  sourceType: { type: String, default: PAYMENT_SOURCE.ACCOUNT },
  sourceAccountId: { type: String, default: null },
  accounts: { type: Array, default: () => [] },
  /** Account-sourced payments must match the target's currency. */
  currency: { type: String, default: null },
  error: { type: [Array, String], default: null }
})

const emit = defineEmits(['update:sourceType', 'update:sourceAccountId'])

const { format } = useMoney()

const isAccount = computed(() => props.sourceType === PAYMENT_SOURCE.ACCOUNT)

const eligibleAccounts = computed(() =>
  props.accounts
    .filter((account) => !props.currency || account.currency === props.currency)
    .map((account) => ({
      value: account.id,
      // Blocked accounts refuse withdrawals, so they cannot fund a payment.
      label: `${account.name} — ${format(account.balance, { currency: account.currency })}${account.isBlockedForSaving ? ' (blocked)' : ''}`,
      disabled: account.isBlockedForSaving
    }))
)

function choose(type) {
  emit('update:sourceType', type)
  // The API rejects an account id on an external payment, so clear it.
  if (type === PAYMENT_SOURCE.EXTERNAL) emit('update:sourceAccountId', null)
}

watch(() => props.currency, () => {
  if (isAccount.value && props.sourceAccountId) {
    const stillValid = eligibleAccounts.value.some((option) => option.value === props.sourceAccountId)
    if (!stillValid) emit('update:sourceAccountId', null)
  }
})
</script>

<template>
  <div class="source">
    <span class="source__label">{{ t('common.paidFrom') }}</span>

    <div class="source__options" role="radiogroup" :aria-label="t('common.paymentSource')">
      <button
        type="button"
        role="radio"
        :aria-checked="isAccount"
        :class="['source__option', { 'is-selected': isAccount }]"
        @click="choose(PAYMENT_SOURCE.ACCOUNT)"
      >
        <BaseIcon name="wallet" :size="16" />
        <span class="source__option-title">{{ t('common.myAccounts') }}</span>
        <span class="source__option-note">{{ t('common.myAccountsNote') }}</span>
      </button>

      <button
        type="button"
        role="radio"
        :aria-checked="!isAccount"
        :class="['source__option', { 'is-selected': !isAccount }]"
        @click="choose(PAYMENT_SOURCE.EXTERNAL)"
      >
        <BaseIcon name="receipt" :size="16" />
        <span class="source__option-title">{{ t('common.external') }}</span>
        <span class="source__option-note">{{ t('common.externalNote') }}</span>
      </button>
    </div>

    <BaseSelect
      v-if="isAccount"
      :model-value="sourceAccountId"
      :label="t('common.account')"
      :options="eligibleAccounts"
      :placeholder="eligibleAccounts.length ? t('accounts.chooseAccount') : t('common.noAccountsInCurrency', { currency })"
      required
      :error="error"
      @update:model-value="emit('update:sourceAccountId', $event)"
    />
  </div>
</template>

<style scoped lang="scss">
.source { display: flex; flex-direction: column; gap: var(--sp-2); }

.source__label { font-size: var(--fs-sm); font-weight: var(--fw-medium); }

.source__options {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--sp-2);
  margin-bottom: var(--sp-2);
}

.source__option {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 2px;

  padding: var(--sp-3);
  border: var(--border);
  border-radius: var(--radius);
  background: var(--surface);
  text-align: left;
  cursor: pointer;

  @include focus-ring;
  transition: background var(--dur-fast) var(--ease);

  &:hover { background: var(--canvas-alt); }

  &.is-selected {
    background: var(--canvas-alt);
    box-shadow: var(--shadow-sm);
    border-color: var(--accent);
  }
}

.source__option-title {
  font-size: var(--fs-sm);
  font-weight: var(--fw-semibold);
  margin-top: var(--sp-1);
}

.source__option-note { font-size: var(--fs-xs); color: var(--text-muted); }

@media (max-width: 480px) {
  .source__options { grid-template-columns: 1fr; }
}
</style>
