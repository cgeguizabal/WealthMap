<script setup>
import { useMoney } from '@/composables/useMoney'
import BaseTable from '@/components/base/BaseTable.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'

const props = defineProps({
  payments: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false },
  /** Hidden on a card/debt detail view where every row shares one target. */
  showTarget: { type: Boolean, default: true },
  emptyMessage: { type: String, default: 'Payments from any source appear here, including cash.' }
})

const { format } = useMoney()

const COLUMNS = [
  { key: 'occurredAt', label: 'Date', width: '150px' },
  { key: 'targetType', label: 'Paid', width: '140px' },
  { key: 'sourceType', label: 'Source', width: '160px' },
  { key: 'notes', label: 'Notes', hideOnMobile: true },
  { key: 'amount', label: 'Amount', align: 'right', width: '130px' }
]

const columns = props.showTarget ? COLUMNS : COLUMNS.filter((c) => c.key !== 'targetType')

const TARGET_LABEL = { CreditCard: 'Credit card', Debt: 'Debt', Installment: 'Installment' }

function formatDate(iso) {
  return new Date(iso).toLocaleDateString(undefined, {
    year: 'numeric', month: 'short', day: '2-digit'
  })
}
</script>

<template>
  <BaseTable
    :columns="columns"
    :rows="payments"
    :loading="loading"
    empty-title="No payments recorded"
    :empty-message="emptyMessage"
  >
    <template #cell-occurredAt="{ value }">
      <span class="numeric muted">{{ formatDate(value) }}</span>
    </template>

    <template #cell-targetType="{ value }">
      <span class="muted">{{ TARGET_LABEL[value] ?? value }}</span>
    </template>

    <!-- External payments have no account, which is exactly why this ledger exists -->
    <template #cell-sourceType="{ value }">
      <BaseBadge :variant="value === 'External' ? 'neutral' : 'accent'" size="sm">
        {{ value === 'External' ? 'Cash / third party' : 'From account' }}
      </BaseBadge>
    </template>

    <template #cell-notes="{ value }">
      <span class="muted">{{ value || '—' }}</span>
    </template>

    <template #cell-amount="{ row }">
      <span class="numeric amount">{{ format(row.amount, { currency: row.currency }) }}</span>
    </template>
  </BaseTable>
</template>

<style scoped lang="scss">
.muted { color: var(--text-muted); font-size: var(--fs-sm); }
.amount { font-weight: var(--fw-semibold); }
</style>
