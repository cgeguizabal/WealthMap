<script setup>
import { computed } from 'vue'
import { useMoney } from '@/composables/useMoney'
import { useI18n } from '@/composables/useI18n'
import { useServerText } from '@/composables/useServerText'
import BaseTable from '@/components/base/BaseTable.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseTimestamp from '@/components/base/BaseTimestamp.vue'

const props = defineProps({
  payments: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false },
  /** Hidden on a card/debt detail view where every row shares one target. */
  showTarget: { type: Boolean, default: true },
  /** Null falls back to the translated default, which a literal default could not. */
  emptyMessage: { type: String, default: null }
})

const { format } = useMoney()
const { t } = useI18n()

/**
 * Computed rather than a const so headers follow the language selector, and so
 * showTarget is re-evaluated instead of being read once at setup.
 */
const columns = computed(() => {
  const all = [
    { key: 'occurredAt', label: t('common.date'), width: '150px' },
    { key: 'targetType', label: t('payments.paid'), width: '140px' },
    { key: 'sourceType', label: t('payments.source'), width: '160px' },
    { key: 'notes', label: t('common.notes'), hideOnMobile: true },
    { key: 'amount', label: t('common.amount'), align: 'right', width: '130px' }
  ]

  return props.showTarget ? all : all.filter((c) => c.key !== 'targetType')
})

const emptyText = computed(() => props.emptyMessage ?? t('payments.emptyMessage'))

const { label: serverLabel } = useServerText()
</script>

<template>
  <BaseTable
    :columns="columns"
    :rows="payments"
    :loading="loading"
    :empty-title="t('payments.emptyTitle')"
    :empty-message="emptyText"
  >
    <template #cell-occurredAt="{ value }">
      <BaseTimestamp :value="value" />
    </template>

    <template #cell-targetType="{ value }">
      <span class="muted">{{ serverLabel('paymentTarget', value) }}</span>
    </template>

    <!-- External payments have no account, which is exactly why this ledger exists -->
    <template #cell-sourceType="{ value }">
      <BaseBadge :variant="value === 'External' ? 'neutral' : 'accent'" size="sm">
        {{ serverLabel('paymentSource', value) }}
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
