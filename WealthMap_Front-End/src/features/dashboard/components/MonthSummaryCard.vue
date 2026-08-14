<script setup>
import { computed } from 'vue'
import { useMoney } from '@/composables/useMoney'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseProgress from '@/components/base/BaseProgress.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import { useI18n } from '@/composables/useI18n'
import { useDateTime } from '@/composables/useDateTime'

const { t } = useI18n()
const { formatDate } = useDateTime()

const props = defineProps({
  data: { type: Object, required: true }
})

const { format, formatPercent } = useMoney()

/** The date the figure is safe until — the furthest bill the projection covers. */
const horizon = computed(() => formatDate(props.data.safeToSpendHorizon))

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
    <!-- Not a subtraction: the total is the lowest point the balance reaches on
         the way to the horizon, so these rows are the inputs to that walk rather
         than terms that add up to it. The horizon line is what makes the figure
         readable — "safe until this date" is the actual claim. -->
    <dl class="month">
      <div class="month__row">
        <dt>
          {{ t('dashboard.spendableCash') }}
          <span class="month__note">{{ t('composed.spendableCashNote') }}</span>
        </dt>
        <dd class="numeric">{{ format(data.spendableCash) }}</dd>
      </div>

      <!-- Shown because the total can exceed cash: a card charged today is not
           settled until its statement falls due. -->
      <div v-if="data.totalAvailableCredit > 0" class="month__row">
        <dt>
          {{ t('dashboard.availableCredit') }}
          <span class="month__note">{{ t('composed.spendableOnCards') }}</span>
        </dt>
        <dd class="numeric">{{ format(data.totalAvailableCredit) }}</dd>
      </div>

      <div v-if="data.incomingBeforeHorizon > 0" class="month__row">
        <dt>
          {{ t('dashboard.incomingSalary') }}
          <span class="month__note">{{ t('composed.beforeDate', { date: horizon }) }}</span>
        </dt>
        <dd class="numeric is-positive">{{ format(data.incomingBeforeHorizon) }}</dd>
      </div>

      <div class="month__row">
        <dt>
          {{ t('dashboard.fallingDue') }}
          <span class="month__note">{{ t('composed.cardsLoansInstallments') }}</span>
        </dt>
        <dd class="numeric">{{ format(data.committedBeforeHorizon) }}</dd>
      </div>

      <div class="month__row month__row--total">
        <dt>
          {{ t('dashboard.safeToSpend') }}
          <span class="month__note">{{ t('composed.safeUntil', { date: horizon }) }}</span>
        </dt>
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
