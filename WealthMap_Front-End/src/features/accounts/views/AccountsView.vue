<script setup>
import { ref, computed, onMounted } from 'vue'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { accountsApi } from '@/api/accounts.api'
import { useAsync } from '@/composables/useAsync'
import { useToast } from '@/composables/useToast'
import { useMoney } from '@/composables/useMoney'
import { useDoubleConfirm } from '@/composables/useDoubleConfirm'
import { useI18n } from '@/composables/useI18n'
import { useDashboardStore } from '@/stores/dashboard.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'
import CardGridSkeleton from '@/features/shared/components/CardGridSkeleton.vue'

import AccountCard from '../components/AccountCard.vue'
import AccountFormModal from '../components/AccountFormModal.vue'
import MovementFormModal from '../components/MovementFormModal.vue'
import TransferModal from '../components/TransferModal.vue'

const { t } = useI18n()
const toast = useToast()
const { format } = useMoney()
const confirmTwice = useDoubleConfirm()
const dashboard = useDashboardStore()

const { data: accounts, loading, error, run: loadAccounts } = useAsync(accountsApi.list, { initialData: [] })

const formOpen = ref(false)
const editing = ref(null)
const movementOpen = ref(false)
const movementMode = ref('deposit')
const movementAccount = ref(null)
const transferOpen = ref(false)

/** Grouped by currency, because balances in different currencies cannot be summed. */
const totalsByCurrency = computed(() => {
  const totals = new Map()

  for (const account of accounts.value ?? []) {
    totals.set(account.currency, (totals.get(account.currency) ?? 0) + account.balance)
  }

  return [...totals.entries()].map(([currency, total]) => ({ currency, total }))
})

function openCreate() {
  editing.value = null
  formOpen.value = true
}

function openEdit(account) {
  editing.value = account
  formOpen.value = true
}

function openMovement(account, mode) {
  movementAccount.value = account
  movementMode.value = mode
  movementOpen.value = true
}

async function toggleBlock(account) {
  try {
    if (account.isBlockedForSaving) {
      await accountsApi.unblock(account.id)
      toast.success(t('accounts.unblockedToast', { name: account.name }))
    } else {
      await accountsApi.block(account.id)
      toast.success(t('accounts.blockedToast', { name: account.name }))
    }
    refresh()
  } catch (err) {
    toast.error(err.message)
  }
}

/**
 * Deleting archives: the account disappears from every list and total, but its
 * movements stay, and the purchases and payments that reference it are intact.
 * The copy says so, because "delete" otherwise implies the history goes too.
 */
async function remove(account) {
  const balanceNote = account.balance !== 0
    ? t('accounts.deleteBalanceNote', {
        amount: format(account.balance, { currency: account.currency })
      })
    : ''

  const confirmed = await confirmTwice({
    title: t('accounts.deleteTitle', { name: account.name }),
    message: t('accounts.deleteMessage', { name: account.name, balance: balanceNote }),
    secondMessage: t('accounts.deleteSecond', { name: account.name })
  })

  if (!confirmed) return

  try {
    await accountsApi.remove(account.id)
    toast.success(t('accounts.deleted', { name: account.name }))
    refresh()
  } catch (err) {
    toast.error(err.message)
  }
}

/** Any money movement invalidates the dashboard's aggregates. */
function refresh() {
  loadAccounts()
  dashboard.invalidate()
}

onMounted(loadAccounts)
</script>

<template>
  <div>
    <PageHeader :title="t('accounts.title')" :subtitle="t('accounts.subtitle')">
      <template #actions>
        <BaseButton
          variant="secondary"
          :disabled="(accounts?.length ?? 0) < 2"
          :title="(accounts?.length ?? 0) < 2 ? t('accounts.needTwoToTransfer') : undefined"
          @click="transferOpen = true"
        >
          <template #icon><BaseIcon name="transfer" :size="15" /></template>
          {{ t('accounts.transfer') }}
        </BaseButton>

        <BaseButton variant="primary" @click="openCreate">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          {{ t('accounts.newAccount') }}
        </BaseButton>
      </template>
    </PageHeader>

    <div v-if="totalsByCurrency.length" class="totals">
      <div v-for="entry in totalsByCurrency" :key="entry.currency" class="totals__item">
        <span class="totals__label">{{ t('accounts.totalHeld') }}</span>
        <span class="totals__value numeric">{{ format(entry.total, { currency: entry.currency }) }}</span>
      </div>
    </div>

    <CardGridSkeleton v-if="loading && !accounts?.length" />

    <BaseEmptyState
      v-else-if="error"
      icon="alert"
      :title="t('accounts.loadFailed')"
      :message="error.message"
    >
      <template #action><BaseButton variant="primary" @click="loadAccounts">{{ t('common.tryAgain') }}</BaseButton></template>
    </BaseEmptyState>

    <BaseEmptyState
      v-else-if="!accounts?.length"
      icon="wallet"
      :title="t('accounts.noAccountsTitle')"
      :message="t('accounts.noAccountsMessage')"
    >
      <template #action><BaseButton variant="primary" @click="openCreate">{{ t('accounts.addFirst') }}</BaseButton></template>
    </BaseEmptyState>

    <motion.div
      v-else
      class="grid"
      v-bind="fadeUp()"
    >
      <AccountCard
        v-for="account in accounts"
        :key="account.id"
        :account="account"
        @deposit="openMovement($event, 'deposit')"
        @withdraw="openMovement($event, 'withdraw')"
        @edit="openEdit"
        @toggle-block="toggleBlock"
        @delete="remove"
      />
    </motion.div>

    <AccountFormModal v-model="formOpen" :account="editing" @saved="refresh" />

    <MovementFormModal
      v-model="movementOpen"
      :account="movementAccount"
      :mode="movementMode"
      @saved="refresh"
    />

    <TransferModal v-model="transferOpen" :accounts="accounts ?? []" @saved="refresh" />
  </div>
</template>

<style scoped lang="scss" src="./AccountsView.scss"></style>
