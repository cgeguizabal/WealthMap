<script setup>
import { ref, computed, onMounted } from 'vue'
import { motion } from 'motion-v'
import { accountsApi } from '@/api/accounts.api'
import { useAsync } from '@/composables/useAsync'
import { useToast } from '@/composables/useToast'
import { useMoney } from '@/composables/useMoney'
import { useDashboardStore } from '@/stores/dashboard.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseSpinner from '@/components/base/BaseSpinner.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'

import AccountCard from '../components/AccountCard.vue'
import AccountFormModal from '../components/AccountFormModal.vue'
import MovementFormModal from '../components/MovementFormModal.vue'
import TransferModal from '../components/TransferModal.vue'

const toast = useToast()
const { format } = useMoney()
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
      toast.success(`${account.name} unblocked.`)
    } else {
      await accountsApi.block(account.id)
      toast.success(`${account.name} blocked — deposits still work, withdrawals do not.`)
    }
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
    <PageHeader title="Accounts" subtitle="Every balance you hold, and where it sits.">
      <template #actions>
        <BaseButton
          variant="secondary"
          :disabled="(accounts?.length ?? 0) < 2"
          :title="(accounts?.length ?? 0) < 2 ? 'You need two accounts to transfer' : undefined"
          @click="transferOpen = true"
        >
          <template #icon><BaseIcon name="transfer" :size="15" /></template>
          Transfer
        </BaseButton>

        <BaseButton variant="primary" @click="openCreate">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          New account
        </BaseButton>
      </template>
    </PageHeader>

    <div v-if="totalsByCurrency.length" class="totals">
      <div v-for="entry in totalsByCurrency" :key="entry.currency" class="totals__item">
        <span class="totals__label">Total held</span>
        <span class="totals__value numeric">{{ format(entry.total, { currency: entry.currency }) }}</span>
      </div>
    </div>

    <div v-if="loading && !accounts?.length" class="state"><BaseSpinner :size="22" /></div>

    <BaseEmptyState
      v-else-if="error"
      icon="alert"
      title="Could not load your accounts"
      :message="error.message"
    >
      <template #action><BaseButton variant="primary" @click="loadAccounts">Try again</BaseButton></template>
    </BaseEmptyState>

    <BaseEmptyState
      v-else-if="!accounts?.length"
      icon="wallet"
      title="No accounts yet"
      message="Money always lives in an account. Add your first one to start tracking."
    >
      <template #action><BaseButton variant="primary" @click="openCreate">Add an account</BaseButton></template>
    </BaseEmptyState>

    <motion.div
      v-else
      class="grid"
      :initial="{ opacity: 0, y: 8 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.28, ease: [0.2, 0, 0, 1] }"
    >
      <AccountCard
        v-for="account in accounts"
        :key="account.id"
        :account="account"
        @deposit="openMovement($event, 'deposit')"
        @withdraw="openMovement($event, 'withdraw')"
        @edit="openEdit"
        @toggle-block="toggleBlock"
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

<style scoped lang="scss">
.totals {
  display: flex;
  flex-wrap: wrap;
  gap: var(--sp-3);
  margin-bottom: var(--sp-5);
}

.totals__item {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: var(--sp-3) var(--sp-4);
  background: var(--canvas-alt);
  border: var(--border-subtle);
  border-radius: var(--radius);
}

.totals__label {
  font-size: var(--fs-xs);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-muted);
}

.totals__value { font-size: var(--fs-lg); font-weight: var(--fw-semibold); }

.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: var(--sp-4);
}

.state { display: grid; place-items: center; padding: var(--sp-12); color: var(--text-muted); }

@media (max-width: 640px) {
  .grid { grid-template-columns: 1fr; }
}
</style>
