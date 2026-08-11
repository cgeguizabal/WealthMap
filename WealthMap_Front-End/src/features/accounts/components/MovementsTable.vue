<script setup>
import { computed } from 'vue'
import { useMoney } from '@/composables/useMoney'
import { useI18n } from '@/composables/useI18n'
import { useServerText } from '@/composables/useServerText'
import BaseTable from '@/components/base/BaseTable.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BasePagination from '@/components/base/BasePagination.vue'
import BaseTimestamp from '@/components/base/BaseTimestamp.vue'

defineProps({
  movements: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false },
  pagination: { type: Object, default: null }
})

defineEmits(['update:page'])

const { format } = useMoney()
const { t } = useI18n()

/** Computed so the headers follow the language selector rather than freezing. */
const COLUMNS = computed(() => [
  { key: 'occurredAt', label: t('common.date'), width: '150px' },
  { key: 'description', label: t('common.description') },
  { key: 'type', label: t('common.type'), width: '150px' },
  { key: 'amount', label: t('common.amount'), align: 'right', width: '130px' },
  { key: 'balanceAfter', label: t('accounts.balance'), align: 'right', width: '130px' }
])

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

/**
 * Movement types arrive as PascalCase enum names. They are looked up rather
 * than de-camel-cased, because splitting on capitals only ever produces English.
 */
const { label: serverLabel } = useServerText()
</script>

<template>
  <div>
    <BaseTable
      :columns="COLUMNS"
      :rows="movements"
      :loading="loading"
      :empty-title="t('accounts.noMovementsTitle')"
      :empty-message="t('accounts.noMovementsMessage')"
    >
      <template #cell-occurredAt="{ value }">
        <BaseTimestamp :value="value" />
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
          {{ serverLabel('movementType', row.type) }}
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
