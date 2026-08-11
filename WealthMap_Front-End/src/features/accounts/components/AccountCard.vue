<script setup>
import { RouterLink } from 'vue-router'
import { useMoney } from '@/composables/useMoney'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'
import { useServerText } from '@/composables/useServerText'

const { t } = useI18n()
const { label: serverLabel } = useServerText()

defineProps({
  account: { type: Object, required: true }
})

defineEmits(['deposit', 'withdraw', 'edit', 'toggle-block', 'delete'])

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
          {{ serverLabel('accountType', account.type) }}
        </BaseBadge>
      </header>

      <p class="account__balance numeric">
        {{ format(account.balance, { currency: account.currency }) }}
      </p>

      <BaseBadge v-if="account.isBlockedForSaving" variant="warning" size="sm">
        {{ t('accounts.blocked') }}
      </BaseBadge>

      <p v-if="account.notes" class="account__notes">{{ account.notes }}</p>
    </RouterLink>

    <footer class="account__actions">
      <BaseButton size="sm" variant="secondary" @click="$emit('deposit', account)">
        <template #icon><BaseIcon name="plus" :size="14" /></template>
        {{ t('accounts.deposit') }}
      </BaseButton>

      <BaseButton
        size="sm"
        variant="secondary"
        :disabled="account.isBlockedForSaving"
        :title="account.isBlockedForSaving ? t('accounts.unblockToWithdraw') : undefined"
        @click="$emit('withdraw', account)"
      >
        <template #icon><BaseIcon name="minus" :size="14" /></template>
        {{ t('accounts.withdraw') }}
      </BaseButton>

      <div class="account__spacer" />

      <BaseButton
        size="sm"
        variant="ghost"
        :title="account.isBlockedForSaving ? t('accounts.unblock') : t('accounts.block')"
        :aria-label="account.isBlockedForSaving ? t('accounts.unblockAria') : t('accounts.blockAria')"
        @click="$emit('toggle-block', account)"
      >
        <template #icon>
          <BaseIcon :name="account.isBlockedForSaving ? 'unlock' : 'lock'" :size="14" />
        </template>
      </BaseButton>

      <BaseButton
        size="sm"
        variant="ghost"
        :title="t('common.edit')"
        :aria-label="t('accounts.editAccount')"
        @click="$emit('edit', account)"
      >
        <template #icon><BaseIcon name="pencil" :size="14" /></template>
      </BaseButton>

      <BaseButton
        class="account__delete"
        size="sm"
        variant="ghost"
        :title="t('common.delete')"
        :aria-label="t('accounts.deleteAria')"
        @click="$emit('delete', account)"
      >
        <template #icon><BaseIcon name="trash" :size="14" /></template>
      </BaseButton>
    </footer>
  </article>
</template>

<style scoped lang="scss" src="./AccountCard.scss"></style>
