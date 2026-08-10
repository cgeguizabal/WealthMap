<script setup>
import { useMoney } from '@/composables/useMoney'
import BaseTable from '@/components/base/BaseTable.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BasePagination from '@/components/base/BasePagination.vue'

defineProps({
  movements: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false },
  pagination: { type: Object, default: null }
})

defineEmits(['update:page'])

const { format } = useMoney()

const COLUMNS = [
  { key: 'occurredAt', label: 'Date', width: '150px' },
  { key: 'description', label: 'Description' },
  { key: 'type', label: 'Type', width: '150px' },
  { key: 'amount', label: 'Amount', align: 'right', width: '130px' },
  { key: 'balanceAfter', label: 'Balance', align: 'right', width: '130px' }
]

const ICON_BY_TYPE = {
  SalaryDeposit: 'briefcase',
  Deposit: 'arrow-down-left',
  Bonus: 'plus',
  TransferIn: 'arrow-down-left',
  TransferOut: 'arrow-up-right',
  Purchase: 'bag',
  Payment: 'receipt',
  AtmWithdrawal: 'arrow-up-right'
}

/** PascalCase enum names read poorly in a table. */
function humanize(value) {
  return value.replace(/([A-Z])/g, ' $1').trim()
}

function formatDate(iso) {
  return new Date(iso).toLocaleDateString(undefined, {
    year: 'numeric', month: 'short', day: '2-digit'
  })
}
</script>

<template>
  <div>
    <BaseTable
      :columns="COLUMNS"
      :rows="movements"
      :loading="loading"
      empty-title="No movements yet"
      empty-message="Deposits, withdrawals, transfers and payments all appear here."
    >
      <template #cell-occurredAt="{ value }">
        <span class="numeric muted">{{ formatDate(value) }}</span>
      </template>

      <template #cell-description="{ row }">
        <div class="desc">
          <span class="desc__text">{{ row.description }}</span>
          <span v-if="row.location" class="desc__location">{{ row.location }}</span>
        </div>
      </template>

      <template #cell-type="{ row }">
        <span class="type">
          <BaseIcon :name="ICON_BY_TYPE[row.type] ?? 'clock'" :size="14" />
          {{ humanize(row.type) }}
        </span>
      </template>

      <template #cell-amount="{ row }">
        <span class="numeric amount" :class="row.isInbound ? 'is-in' : 'is-out'">
          {{ row.isInbound ? '+' : '−' }}{{ format(row.amount, { currency: row.currency }) }}
        </span>
      </template>

      <!-- BalanceAfter is a stored fact, not a running total computed here -->
      <template #cell-balanceAfter="{ row }">
        <span class="numeric muted">{{ format(row.balanceAfter, { currency: row.currency }) }}</span>
      </template>
    </BaseTable>

    <BasePagination
      v-if="pagination"
      :page="pagination.page"
      :page-size="pagination.pageSize"
      :total-count="pagination.totalCount"
      :total-pages="pagination.totalPages"
      :has-next-page="pagination.hasNextPage"
      :has-previous-page="pagination.hasPreviousPage"
      @update:page="$emit('update:page', $event)"
    />
  </div>
</template>

<style scoped lang="scss">
.muted { color: var(--text-muted); font-size: var(--fs-sm); }

.desc { display: flex; flex-direction: column; }
.desc__text { font-weight: var(--fw-medium); }
.desc__location { font-size: var(--fs-xs); color: var(--text-muted); }

.type {
  display: inline-flex;
  align-items: center;
  gap: var(--sp-2);
  font-size: var(--fs-sm);
  color: var(--text-muted);
}

.amount { font-weight: var(--fw-semibold); }
.amount.is-in { color: var(--positive); }
.amount.is-out { color: var(--negative); }
</style>
