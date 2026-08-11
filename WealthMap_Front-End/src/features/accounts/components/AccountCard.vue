<script setup>
import { RouterLink } from 'vue-router'
import { useMoney } from '@/composables/useMoney'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseButton from '@/components/base/BaseButton.vue'

defineProps({
  account: { type: Object, required: true }
})

defineEmits(['deposit', 'withdraw', 'edit', 'toggle-block'])

const { format } = useMoney()
</script>

<template>
  <article class="account">
    <RouterLink :to="`/accounts/${account.id}`" class="account__main">
      <header class="account__head">
        <div class="account__identity">
          <BaseIcon :name="account.type === 'Savings' ? 'lock' : 'wallet'" :size="16" />
          <div>
            <h3 class="account__name">{{ account.name }}</h3>
            <p class="account__bank">{{ account.bankName }}</p>
          </div>
        </div>

        <BaseBadge :variant="account.type === 'Savings' ? 'accent' : 'neutral'" size="sm">
          {{ account.type }}
        </BaseBadge>
      </header>

      <p class="account__balance numeric">
        {{ format(account.balance, { currency: account.currency }) }}
      </p>

      <BaseBadge v-if="account.isBlockedForSaving" variant="warning" size="sm">
        Blocked for saving
      </BaseBadge>

      <p v-if="account.notes" class="account__notes">{{ account.notes }}</p>
    </RouterLink>

    <footer class="account__actions">
      <BaseButton size="sm" variant="secondary" @click="$emit('deposit', account)">
        <template #icon><BaseIcon name="plus" :size="14" /></template>
        Deposit
      </BaseButton>

      <BaseButton
        size="sm"
        variant="secondary"
        :disabled="account.isBlockedForSaving"
        :title="account.isBlockedForSaving ? 'Unblock the account to withdraw' : undefined"
        @click="$emit('withdraw', account)"
      >
        <template #icon><BaseIcon name="minus" :size="14" /></template>
        Withdraw
      </BaseButton>

      <div class="account__spacer" />

      <BaseButton
        size="sm"
        variant="ghost"
        :title="account.isBlockedForSaving ? 'Unblock' : 'Block for saving'"
        @click="$emit('toggle-block', account)"
      >
        <template #icon>
          <BaseIcon :name="account.isBlockedForSaving ? 'unlock' : 'lock'" :size="14" />
        </template>
        <span class="sr-only">{{ account.isBlockedForSaving ? 'Unblock' : 'Block' }}</span>
      </BaseButton>

      <BaseButton size="sm" variant="ghost" title="Edit" @click="$emit('edit', account)">
        <template #icon><BaseIcon name="pencil" :size="14" /></template>
        <span class="sr-only">Edit</span>
      </BaseButton>
    </footer>
  </article>
</template>

<style scoped lang="scss">
.account {
  display: flex;
  flex-direction: column;

  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
  overflow: hidden;
}

.account__main {
  display: flex;
  flex-direction: column;
  gap: var(--sp-2);
  padding: var(--sp-4) var(--sp-5);
  color: inherit;
  text-decoration: none;

  &:hover { background: var(--canvas-alt); text-decoration: none; }
}

.account__head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--sp-3);
}

.account__identity {
  display: flex;
  align-items: center;
  gap: var(--sp-3);
  min-width: 0;
  color: var(--text-muted);
}

.account__name {
  font-size: var(--fs-md);
  font-weight: var(--fw-semibold);
  color: var(--text);
  @include truncate;
}

.account__bank { font-size: var(--fs-xs); color: var(--text-muted); }

.account__balance {
  font-size: var(--fs-xl);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.02em;
}

.account__notes {
  font-size: var(--fs-xs);
  color: var(--text-muted);
  @include truncate;
}

.account__actions {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: var(--sp-2);
  padding: var(--sp-3) var(--sp-4);
  border-top: var(--border-subtle);
  background: var(--canvas-alt);
}

.account__spacer { flex: 1; }
</style>
