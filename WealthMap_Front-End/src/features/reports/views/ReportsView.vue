<script setup>
import { ref, computed, onMounted } from 'vue'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { reportsApi, currentMonth, downloadBlob } from '@/api/reports.api'
import { useMoney } from '@/composables/useMoney'
import { useToast } from '@/composables/useToast'
import { GOAL_STATUS_VARIANT } from '@/api/goals.api'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseTable from '@/components/base/BaseTable.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseSpinner from '@/components/base/BaseSpinner.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'

import ReportSpending from '../components/ReportSpending.vue'
import { useI18n } from '@/composables/useI18n'
import { useServerText } from '@/composables/useServerText'

const { t } = useI18n()
const { label: serverLabel } = useServerText()

const { format } = useMoney()
const toast = useToast()

const month = ref(currentMonth())
const report = ref(null)
const loading = ref(false)
const downloading = ref(false)
const error = ref(null)

/** Computed so the headers follow the language selector rather than freezing. */
const ACCOUNT_COLUMNS = computed(() => [
  { key: 'name', label: t('reports.colAccount') },
  { key: 'openingBalance', label: t('reports.colOpening'), align: 'right' },
  { key: 'totalIn', label: t('reports.colIn'), align: 'right' },
  { key: 'totalOut', label: t('reports.colOut'), align: 'right' },
  { key: 'closingBalance', label: t('reports.colClosing'), align: 'right' }
])

const CARD_COLUMNS = computed(() => [
  { key: 'cardName', label: t('reports.colCard') },
  { key: 'chargedThisMonth', label: t('reports.colCharged'), align: 'right' },
  { key: 'paidThisMonth', label: t('reports.colPaid'), align: 'right' },
  { key: 'usedCredit', label: t('reports.colOwed'), align: 'right' },
  { key: 'availableCredit', label: t('reports.colAvailable'), align: 'right', hideOnMobile: true }
])

const monthLabel = computed(() => {
  if (!report.value) return ''
  return new Date(`${report.value.month}-01T00:00:00`).toLocaleDateString(undefined, {
    month: 'long', year: 'numeric'
  })
})

async function load() {
  loading.value = true
  error.value = null

  try {
    report.value = await reportsApi.monthly(month.value)
  } catch (err) {
    error.value = err
    report.value = null
  } finally {
    loading.value = false
  }
}

async function downloadPdf() {
  downloading.value = true

  try {
    const blob = await reportsApi.monthlyPdf(month.value)
    downloadBlob(blob, `wealthmap-${month.value}.pdf`)
    toast.success(t('reports.downloaded'))
  } catch {
    // A failed blob request has no JSON body to read a message from.
    toast.error(t('reports.downloadFailed'))
  } finally {
    downloading.value = false
  }
}

onMounted(load)
</script>

<template>
  <div>
    <PageHeader :title="t('reports.title')" :subtitle="t('reports.subtitle')">
      <template #actions>
        <input v-model="month" class="month-input" type="month" :aria-label="t('reports.reportMonth')" />

        <BaseButton variant="secondary" :loading="loading" @click="load">{{ t('reports.view') }}</BaseButton>

        <BaseButton variant="primary" :loading="downloading" :disabled="!report" @click="downloadPdf">
          <template #icon><BaseIcon name="download" :size="15" /></template>
          {{ t('reports.pdf') }}
        </BaseButton>
      </template>
    </PageHeader>

    <div v-if="loading && !report" class="state"><BaseSpinner :size="22" /></div>

    <BaseEmptyState
      v-else-if="error"
      icon="alert"
      :title="t('reports.loadFailed')"
      :message="error.message"
    >
      <template #action><BaseButton variant="primary" @click="load">{{ t('common.tryAgain') }}</BaseButton></template>
    </BaseEmptyState>

    <motion.div
      v-else-if="report"
      class="report"
      v-bind="fadeUp()"
    >
      <header class="masthead">
        <div>
          <h2 class="masthead__month">{{ monthLabel }}</h2>
          <p class="masthead__period">
            {{ report.periodStart }} → {{ report.periodEnd }} · all amounts in {{ report.currency }}
          </p>
        </div>

        <div class="masthead__net" :class="{ 'is-negative': report.netResult < 0 }">
          <span class="masthead__net-label">{{ t('reports.netResult') }}</span>
          <span class="masthead__net-value numeric">{{ format(report.netResult, { currency: report.currency }) }}</span>
        </div>
      </header>

      <section class="totals">
        <div class="total total--in">
          <span class="total__label">{{ t('reports.income') }}</span>
          <span class="total__value numeric">{{ format(report.income.total, { currency: report.currency }) }}</span>
        </div>
        <div class="total total--out">
          <span class="total__label">{{ t('reports.spending') }}</span>
          <span class="total__value numeric">
            {{ format(report.spending.totalPurchases, { currency: report.currency }) }}
          </span>
        </div>
      </section>

      <!-- ── Income ──────────────────────────── -->
      <BaseCard :title="t('reports.income')" :padded="false">
        <BaseEmptyState
          v-if="!report.income.lines.length"
          icon="arrow-down-left"
          :title="t('reports.noIncomeTitle')"
          :message="t('reports.noIncomeMessage')"
          compact
        />

        <ul v-else class="lines">
          <li v-for="line in report.income.lines" :key="line.type" class="line">
            <span class="line__label">{{ line.type.replace(/([A-Z])/g, ' $1').trim() }}</span>
            <span class="line__count">{{ line.count }}×</span>
            <span class="line__value numeric">{{ format(line.total, { currency: report.currency }) }}</span>
          </li>
        </ul>

        <template v-if="report.income.expectedSalaryNet > 0" #footer>
          <span class="footnote">
            Expected net salary
            <strong class="numeric">{{ format(report.income.expectedSalaryNet, { currency: report.currency }) }}</strong>
            per month
          </span>
        </template>
      </BaseCard>

      <!-- ── Spending ────────────────────────── -->
      <ReportSpending :spending="report.spending" :currency="report.currency" />

      <!-- ── Accounts ────────────────────────── -->
      <BaseCard :title="t('reports.accountsTitle')" :subtitle="t('reports.accountsSubtitle')" :padded="false">
        <BaseTable
          :columns="ACCOUNT_COLUMNS"
          :rows="report.accounts"
          row-key="accountId"
          :empty-title="t('reports.noAccountsTitle')"
          :empty-message="t('reports.noAccountsMessage')"
        >
          <template #cell-name="{ row }">
            <div class="cell-stack">
              <span class="cell-stack__title">{{ row.name }}</span>
              <span class="cell-stack__sub">{{ serverLabel('accountType', row.type) }} · {{ row.movementCount }}</span>
            </div>
          </template>

          <template #cell-openingBalance="{ value }">
            <span class="numeric muted">{{ format(value, { currency: report.currency }) }}</span>
          </template>

          <template #cell-totalIn="{ value }">
            <span class="numeric is-in">{{ format(value, { currency: report.currency }) }}</span>
          </template>

          <template #cell-totalOut="{ value }">
            <span class="numeric is-out">{{ format(value, { currency: report.currency }) }}</span>
          </template>

          <template #cell-closingBalance="{ value }">
            <span class="numeric strong">{{ format(value, { currency: report.currency }) }}</span>
          </template>
        </BaseTable>
      </BaseCard>

      <!-- ── Cards ───────────────────────────── -->
      <BaseCard
        v-if="report.cards.length"
        :title="t('reports.cardsTitle')"
        :subtitle="t('reports.cardsSubtitle')"
        :padded="false"
      >
        <BaseTable :columns="CARD_COLUMNS" :rows="report.cards" row-key="cardId">
          <template #cell-cardName="{ row }">
            <div class="cell-stack">
              <span class="cell-stack__title">{{ row.cardName }}</span>
              <span class="cell-stack__sub">Due day {{ row.paymentDueDay }}</span>
            </div>
          </template>

          <template #cell-chargedThisMonth="{ value }">
            <span class="numeric is-out">{{ format(value, { currency: report.currency }) }}</span>
          </template>

          <template #cell-paidThisMonth="{ value }">
            <span class="numeric is-in">{{ format(value, { currency: report.currency }) }}</span>
          </template>

          <template #cell-usedCredit="{ value }">
            <span class="numeric strong">{{ format(value, { currency: report.currency }) }}</span>
          </template>

          <template #cell-availableCredit="{ value }">
            <span class="numeric muted">{{ format(value, { currency: report.currency }) }}</span>
          </template>
        </BaseTable>
      </BaseCard>

      <!-- ── Goals ───────────────────────────── -->
      <BaseCard v-if="report.goals.length" :title="t('goals.title')" :padded="false">
        <ul class="goals">
          <li v-for="goal in report.goals" :key="`${goal.kind}-${goal.name}`" class="goal">
            <div class="goal__body">
              <span class="goal__name">{{ goal.name }}</span>
              <span class="goal__kind">{{ serverLabel('goalKind', goal.kind) }}</span>
            </div>

            <span class="goal__amounts numeric">
              {{ format(goal.currentAmount, { currency: report.currency }) }}
              <span class="goal__of">of {{ format(goal.targetAmount, { currency: report.currency }) }}</span>
            </span>

            <span class="goal__percent numeric">{{ goal.progressPercentage.toFixed(1) }}%</span>

            <BaseBadge :variant="GOAL_STATUS_VARIANT[goal.status] ?? 'neutral'" size="sm">
              {{ serverLabel('goalStatus', goal.status) }}
            </BaseBadge>
          </li>
        </ul>
      </BaseCard>

      <p class="generated">
        Generated {{ new Date(report.generatedAt).toLocaleString() }} · figures cover
        {{ report.currency }} holdings only
      </p>
    </motion.div>
  </div>
</template>

<style scoped lang="scss">
.month-input {
  height: 36px;
  padding: 0 var(--sp-3);
  border: var(--border);
  border-radius: var(--radius);
  background: var(--surface);
  font-size: var(--fs-base);

  @include focus-ring;
}

.report { display: flex; flex-direction: column; gap: var(--sp-4); }

.masthead {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--sp-4);

  padding: var(--sp-5);
  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
}

.masthead__month { font-size: var(--fs-xl); font-weight: var(--fw-semibold); letter-spacing: -0.02em; }
.masthead__period { font-size: var(--fs-sm); color: var(--text-muted); }

.masthead__net { text-align: right; }

.masthead__net-label {
  display: block;
  font-size: var(--fs-xs);
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--text-muted);
}

.masthead__net-value {
  font-size: var(--fs-2xl);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.02em;
  color: var(--positive);
}

.masthead__net.is-negative .masthead__net-value { color: var(--negative); }

.totals { display: grid; grid-template-columns: 1fr 1fr; gap: var(--sp-4); }

.total {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: var(--sp-4) var(--sp-5);
  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow-sm);
}

.total__label {
  font-size: var(--fs-xs);
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--text-muted);
}

.total__value { font-size: var(--fs-xl); font-weight: var(--fw-semibold); }
.total--in .total__value { color: var(--positive); }
.total--out .total__value { color: var(--negative); }

.lines { display: flex; flex-direction: column; }

.line {
  display: flex;
  align-items: baseline;
  gap: var(--sp-3);
  padding: var(--sp-3) var(--sp-5);
  border-bottom: var(--border-subtle);

  &:last-child { border-bottom: none; }
}

.line__label { flex: 1; font-weight: var(--fw-medium); }
.line__count { font-size: var(--fs-xs); color: var(--text-muted); }
.line__value { font-weight: var(--fw-semibold); }

.footnote {
  font-size: var(--fs-sm);
  color: var(--text-muted);
  strong { color: var(--text); }
}

.cell-stack { display: flex; flex-direction: column; }
.cell-stack__title { font-weight: var(--fw-medium); }
.cell-stack__sub { font-size: var(--fs-xs); color: var(--text-muted); }

.muted { color: var(--text-muted); }
.strong { font-weight: var(--fw-semibold); }
.is-in { color: var(--positive); }
.is-out { color: var(--negative); }

.goals { display: flex; flex-direction: column; }

.goal {
  display: flex;
  align-items: center;
  gap: var(--sp-3);
  padding: var(--sp-3) var(--sp-5);
  border-bottom: var(--border-subtle);

  &:last-child { border-bottom: none; }
}

.goal__body { flex: 1; display: flex; flex-direction: column; min-width: 0; }
.goal__name { font-weight: var(--fw-medium); @include truncate; }
.goal__kind { font-size: var(--fs-xs); color: var(--text-muted); }
.goal__amounts { font-size: var(--fs-sm); }
.goal__of { color: var(--text-muted); }
.goal__percent { font-weight: var(--fw-semibold); min-width: 52px; text-align: right; }

.generated { font-size: var(--fs-xs); color: var(--text-subtle); text-align: center; }

.state { display: grid; place-items: center; padding: var(--sp-12); color: var(--text-muted); }

@media (max-width: 767px) {
  .masthead { flex-direction: column; }
  .masthead__net { text-align: left; }
  .totals { grid-template-columns: 1fr; }
  .line, .goal { padding-left: var(--sp-4); padding-right: var(--sp-4); }
  .goal { flex-wrap: wrap; }
}
</style>
