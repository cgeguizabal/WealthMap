<script setup>
import { computed, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { useDashboardStore } from '@/stores/dashboard.store'
import { useAuthStore } from '@/stores/auth.store'
import { useMoney } from '@/composables/useMoney'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'

import StatTile from '../components/StatTile.vue'
import AlertsPanel from '../components/AlertsPanel.vue'
import UpcomingDueList from '../components/UpcomingDueList.vue'
import GoalsSummaryCard from '../components/GoalsSummaryCard.vue'
import MonthSummaryCard from '../components/MonthSummaryCard.vue'
import ExcludedCurrenciesNotice from '../components/ExcludedCurrenciesNotice.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const dashboard = useDashboardStore()
const auth = useAuthStore()
const { data, alerts, loading, error } = storeToRefs(dashboard)
const { format } = useMoney()

const firstName = computed(() => auth.user?.fullName?.split(' ')[0] ?? '')

/** Credit utilisation, for the tile footnote. */
const utilisation = computed(() => {
  if (!data.value?.totalCreditLimit) return 0
  return (data.value.totalUsedCredit / data.value.totalCreditLimit) * 100
})

// Re-fetches whenever the cached figures belong to someone else or a mutation
// elsewhere invalidated them.
onMounted(() => {
  if (!dashboard.isFresh) dashboard.load()
})
</script>

<template>
  <div class="dashboard">
    <PageHeader :title="firstName ? t('dashboard.greeting', { name: firstName }) : t('dashboard.title')">
      <template #subtitle>
        Everything below is computed from what you have recorded — nothing is estimated.
      </template>

      <template #actions>
        <BaseButton variant="secondary" :loading="loading" @click="dashboard.load()">
          <template #icon><BaseIcon name="refresh" :size="15" /></template>
          {{ t('common.refresh') }}
        </BaseButton>
      </template>
    </PageHeader>

    <!-- Loading: skeleton tiles rather than a spinner, so the layout does not jump -->
    <div v-if="loading && !data" class="skeleton-grid">
      <div v-for="n in 4" :key="n" class="skeleton skeleton--tile" />
      <div class="skeleton skeleton--panel" />
      <div class="skeleton skeleton--panel" />
    </div>

    <BaseEmptyState
      v-else-if="error && !data"
      icon="alert"
      :title="t('dashboard.loadFailed')"
      :message="error.message"
    >
      <template #action>
        <BaseButton variant="primary" @click="dashboard.load()">{{ t('common.tryAgain') }}</BaseButton>
      </template>
    </BaseEmptyState>

    <div v-else-if="data" class="dashboard__body">
      <ExcludedCurrenciesNotice
        :currencies="data.excludedCurrencies"
        :currency="data.currency"
      />

      <AlertsPanel :alerts="alerts" />

      <motion.section
        class="stats"
      v-bind="fadeUp()"
      >
        <StatTile
          :label="t('dashboard.available')"
          :value="format(data.totalAvailable)"
          icon="wallet"
          tone="positive"
        >
          {{ format(data.totalInChecking) }} checking · {{ format(data.totalInSavings) }} savings
        </StatTile>

        <StatTile
          :label="t('dashboard.availableCredit')"
          :value="format(data.totalAvailableCredit)"
          icon="card"
        >
          {{ format(data.totalUsedCredit) }} used of {{ format(data.totalCreditLimit) }}
          ({{ utilisation.toFixed(0) }}%)
        </StatTile>

        <StatTile
          :label="t('dashboard.totalDebt')"
          :value="format(data.totalDebt)"
          icon="debt"
          tone="negative"
        >
          <!-- installmentRemaining sits inside totalUsedCredit, so it is shown as
               context and never added to the total -->
          {{ format(data.totalLoanDebt) }} loans · {{ format(data.totalUsedCredit) }} cards
          <template v-if="data.installmentRemaining > 0">
            <br />includes {{ format(data.installmentRemaining) }} in installment plans
          </template>
        </StatTile>

        <StatTile
          :label="t('dashboard.safeToSpend')"
          :value="format(data.safeToSpend)"
          icon="target"
          :tone="data.safeToSpend < 0 ? 'negative' : 'accent'"
        >
          {{ t('dashboard.safeToSpendHint') }}
        </StatTile>
      </motion.section>

      <section class="panels">
        <UpcomingDueList :items="data.upcomingDueDates" class="panels__wide" />

        <div class="panels__side">
          <MonthSummaryCard :data="data" />
          <GoalsSummaryCard :goals="data.goals" />
        </div>
      </section>

      <p class="dashboard__worth">
        {{ t('dashboard.netWorth') }}
        <strong class="numeric" :class="{ 'is-negative': data.netWorth < 0 }">
          {{ format(data.netWorth) }}
        </strong>
        <span class="dashboard__worth-note">available minus everything owed</span>
      </p>
    </div>
  </div>
</template>

<style scoped lang="scss">
.dashboard__body {
  display: flex;
  flex-direction: column;
  gap: var(--sp-5);
}

.stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--sp-4);
}

.panels {
  display: grid;
  grid-template-columns: 1.35fr 1fr;
  gap: var(--sp-4);
  align-items: start;
}

.panels__side {
  display: flex;
  flex-direction: column;
  gap: var(--sp-4);
}

.dashboard__worth {
  display: flex;
  align-items: baseline;
  gap: var(--sp-2);
  flex-wrap: wrap;

  padding: var(--sp-4) var(--sp-5);
  background: var(--canvas-alt);
  border: var(--border-subtle);
  border-radius: var(--radius);

  font-size: var(--fs-sm);
  color: var(--text-muted);

  strong {
    font-size: var(--fs-lg);
    font-weight: var(--fw-semibold);
    color: var(--text);

    &.is-negative { color: var(--negative); }
  }
}

.dashboard__worth-note { font-size: var(--fs-xs); color: var(--text-subtle); }

/* ── Skeletons ────────────────────────────── */
.skeleton-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--sp-4);
}

.skeleton {
  background: linear-gradient(90deg, var(--canvas-alt) 25%, var(--line) 50%, var(--canvas-alt) 75%);
  background-size: 200% 100%;
  border: var(--border-subtle);
  border-radius: var(--radius);
  animation: shimmer 1.4s infinite;
}

.skeleton--tile { height: 108px; }
.skeleton--panel { height: 260px; grid-column: span 2; }

@keyframes shimmer {
  to { background-position: -200% 0; }
}

@media (max-width: 1200px) {
  .stats, .skeleton-grid { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 1023px) {
  .panels { grid-template-columns: 1fr; }
}

@media (max-width: 640px) {
  .stats, .skeleton-grid { grid-template-columns: 1fr; }
  .skeleton--panel { grid-column: span 1; }
}

@media (prefers-reduced-motion: reduce) {
  .skeleton { animation: none; }
}
</style>
