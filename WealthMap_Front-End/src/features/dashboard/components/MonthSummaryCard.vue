<script setup>
import { computed } from 'vue'
import { useMoney } from '@/composables/useMoney'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseProgress from '@/components/base/BaseProgress.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const props = defineProps({
  data: { type: Object, required: true }
})

const { format, formatPercent } = useMoney()

const overspending = computed(() =>
  props.data.monthlyNetIncome > 0 && props.data.monthSpending > props.data.monthlyNetIncome
)

/** Mirrors the backend's own threshold for the HighDebtRatio alert. */
const ratioVariant = computed(() => {
  const ratio = props.data.debtRatioPercentage
  if (ratio === null || ratio === undefined) return 'neutral'
  if (ratio > 60) return 'negative'
  if (ratio > 40) return 'warning'
  return 'positive'
})
</script>

<template>
  <BaseCard :title="t('dashboard.thisMonth')">
    <dl class="month">
      <div class="month__row">
        <dt>{{ t('dashboard.netIncome') }}</dt>
        <dd class="numeric">{{ format(data.monthlyNetIncome) }}</dd>
      </div>

      <div class="month__row">
        <dt>
          {{ t('dashboard.committed') }}
          <span class="month__note">{{ t('composed.committedNote') }}</span>
        </dt>
        <dd class="numeric">{{ format(data.monthlyObligations) }}</dd>
      </div>

      <div class="month__row month__row--total">
        <dt>{{ t('dashboard.safeToSpend') }}</dt>
        <dd class="numeric" :class="{ 'is-negative': data.safeToSpend < 0 }">
          {{ format(data.safeToSpend) }}
        </dd>
      </div>
    </dl>

    <div class="month__spending">
      <BaseProgress
        :value="data.monthSpending"
        :max="data.monthlyNetIncome || data.monthSpending || 1"
        :variant="overspending ? 'negative' : 'accent'"
        size="sm"
      >
        <template #label>
          <span class="month__spent-label">{{ t('dashboard.spentSoFar') }}</span>
          <span class="month__spent numeric">{{ format(data.monthSpending) }}</span>
        </template>
      </BaseProgress>

      <BaseBadge v-if="overspending" variant="negative" size="sm">
        {{ t('dashboard.spendingAboveIncome') }}
      </BaseBadge>
    </div>

    <div v-if="data.debtRatioPercentage !== null && data.debtRatioPercentage !== undefined" class="month__ratio">
      <span class="month__ratio-label">{{ t('dashboard.debtRatio') }}</span>
      <BaseBadge :variant="ratioVariant" size="sm">
        {{ formatPercent(data.debtRatioPercentage, 2) }}
      </BaseBadge>
    </div>
  </BaseCard>
</template>

<style scoped lang="scss">
.month { display: flex; flex-direction: column; gap: var(--sp-3); }

.month__row {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: var(--sp-4);

  dt { font-size: var(--fs-sm); color: var(--text-muted); }
  dd { font-size: var(--fs-base); font-weight: var(--fw-medium); }
}

.month__note {
  display: block;
  font-size: var(--fs-xs);
  color: var(--text-subtle);
}

.month__row--total {
  padding-top: var(--sp-3);
  border-top: var(--border-subtle);

  dt { font-weight: var(--fw-semibold); color: var(--text); }
  dd { font-size: var(--fs-lg); font-weight: var(--fw-semibold); }

  .is-negative { color: var(--negative); }
}

.month__spending {
  display: flex;
  flex-direction: column;
  gap: var(--sp-2);
  margin-top: var(--sp-5);
  padding-top: var(--sp-4);
  border-top: var(--border-subtle);
}

.month__spent-label { font-size: var(--fs-sm); color: var(--text-muted); }
.month__spent { font-size: var(--fs-sm); font-weight: var(--fw-semibold); margin-left: var(--sp-2); }

.month__ratio {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--sp-2);
  margin-top: var(--sp-4);
}

.month__ratio-label { font-size: var(--fs-sm); color: var(--text-muted); }
</style>
