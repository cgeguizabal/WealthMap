<script setup>
import { ref, computed, onMounted } from 'vue'
import { motion } from 'motion-v'
import { debtsApi } from '@/api/debts.api'
import { useAsync } from '@/composables/useAsync'
import { useMoney } from '@/composables/useMoney'
import { useToast } from '@/composables/useToast'
import { useUiStore } from '@/stores/ui.store'
import { useDashboardStore } from '@/stores/dashboard.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseSpinner from '@/components/base/BaseSpinner.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'

import DebtCard from '../components/DebtCard.vue'
import DebtFormModal from '../components/DebtFormModal.vue'
import DebtPaymentModal from '../components/DebtPaymentModal.vue'

const { format } = useMoney()
const toast = useToast()
const ui = useUiStore()
const dashboard = useDashboardStore()

const { data: debts, loading, error, run: loadDebts } = useAsync(debtsApi.list, { initialData: [] })

const formOpen = ref(false)
const payOpen = ref(false)
const editing = ref(null)
const active = ref(null)

const outstanding = computed(() => {
  const byCurrency = new Map()

  for (const debt of debts.value ?? []) {
    if (debt.status === 'PaidOff') continue
    byCurrency.set(debt.currency, (byCurrency.get(debt.currency) ?? 0) + debt.remainingAmount)
  }

  return [...byCurrency.entries()].map(([currency, total]) => ({ currency, total }))
})

function openCreate() {
  editing.value = null
  formOpen.value = true
}

function openEdit(debt) {
  editing.value = debt
  formOpen.value = true
}

function openPay(debt) {
  active.value = debt
  payOpen.value = true
}

async function markDefaulted(debt) {
  const confirmed = await ui.confirm({
    title: `Mark ${debt.name} as defaulted?`,
    message: 'It stays in your totals. Registering a payment later returns it to active.',
    confirmLabel: 'Mark defaulted',
    variant: 'danger'
  })

  if (!confirmed) return

  try {
    await debtsApi.markDefaulted(debt.id)
    toast.warning(`${debt.name} marked as defaulted.`)
    refresh()
  } catch (err) {
    toast.error(err.message)
  }
}

async function remove(debt) {
  const confirmed = await ui.confirm({
    title: `Delete ${debt.name}?`,
    message: 'This removes the debt and its history. It cannot be undone.',
    confirmLabel: 'Delete',
    variant: 'danger'
  })

  if (!confirmed) return

  try {
    await debtsApi.remove(debt.id)
    toast.success(`${debt.name} deleted.`)
    refresh()
  } catch (err) {
    toast.error(err.message)
  }
}

function refresh() {
  loadDebts()
  dashboard.invalidate()
}

onMounted(loadDebts)
</script>

<template>
  <div>
    <PageHeader title="Debts" subtitle="Loans and anything else you owe outside a credit card.">
      <template #actions>
        <BaseButton variant="primary" @click="openCreate">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          New debt
        </BaseButton>
      </template>
    </PageHeader>

    <div v-if="outstanding.length" class="totals">
      <div v-for="entry in outstanding" :key="entry.currency" class="totals__item">
        <span class="totals__label">Still owed</span>
        <span class="totals__value numeric">{{ format(entry.total, { currency: entry.currency }) }}</span>
      </div>
    </div>

    <div v-if="loading && !debts?.length" class="state"><BaseSpinner :size="22" /></div>

    <BaseEmptyState
      v-else-if="error"
      icon="alert"
      title="Could not load your debts"
      :message="error.message"
    >
      <template #action><BaseButton variant="primary" @click="loadDebts">Try again</BaseButton></template>
    </BaseEmptyState>

    <BaseEmptyState
      v-else-if="!debts?.length"
      icon="debt"
      title="No debts recorded"
      message="Track a loan to see it in your totals, your safe-to-spend and your upcoming due dates."
    >
      <template #action><BaseButton variant="primary" @click="openCreate">Add a debt</BaseButton></template>
    </BaseEmptyState>

    <motion.div
      v-else
      class="grid"
      :initial="{ opacity: 0, y: 8 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.25, ease: [0.2, 0, 0, 1] }"
    >
      <DebtCard
        v-for="debt in debts"
        :key="debt.id"
        :debt="debt"
        @pay="openPay"
        @edit="openEdit"
        @default="markDefaulted"
        @delete="remove"
      />
    </motion.div>

    <DebtFormModal v-model="formOpen" :debt="editing" @saved="refresh" />
    <DebtPaymentModal v-model="payOpen" :debt="active" @saved="refresh" />
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
