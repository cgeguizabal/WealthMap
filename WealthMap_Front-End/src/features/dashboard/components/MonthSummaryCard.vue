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
    <!-- These four rows are the safe-to-spend subtraction itself, so they must
         add up on screen. Card balances already include installment plans (a
         plan charges the card in full on day one), which is why committed
         payments here are loans only. -->
    <dl class="month">
      <div class="month__row">
        <dt>
          {{ t('dashboard.spendableCash') }}
          <span class="month__note">{{ t('composed.spendableCashNote') }}</span>
        </dt>
        <dd class="numeric">{{ format(data.spendableCash) }}</dd>
      </div>

      <div class="month__row">
        <dt>
          {{ t('dashboard.cardBalances') }}
          <span class="month__note">{{ t('composed.cardBalancesNote') }}</span>
        </dt>
        <dd class="numeric">{{ format(data.totalUsedCredit) }}</dd>
      </div>

      <div class="month__row">
        <dt>
          {{ t('dashboard.loanPayments') }}
          <span class="month__note">{{ t('composed.loanPaymentsNote') }}</span>
        </dt>
        <dd class="numeric">{{ format(data.loanPaymentsDue) }}</dd>
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

<style scoped lang="scss" src="@/assets/styles/features/dashboard/MonthSummaryCard.scss"></style>
