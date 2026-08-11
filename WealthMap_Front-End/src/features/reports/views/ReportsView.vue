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
import { useDateTime } from '@/composables/useDateTime'

const { t } = useI18n()
const { label: serverLabel } = useServerText()
const { formatDateTime } = useDateTime()

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
            {{ t('composed.reportPeriod', {
              start: report.periodStart,
              end: report.periodEnd,
              currency: report.currency
            }) }}
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
            {{ t('reports.expectedNetSalary') }}
            <strong class="numeric">{{ format(report.income.expectedSalaryNet, { currency: report.currency }) }}</strong>
            {{ t('reports.perMonth') }}
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
              <span class="cell-stack__sub">{{ t('composed.reportDueDay', { day: row.paymentDueDay }) }}</span>
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
              <span class="goal__of">{{ t('goals.ofTarget', { amount: format(goal.targetAmount, { currency: report.currency }) }) }}</span>
            </span>

            <span class="goal__percent numeric">{{ goal.progressPercentage.toFixed(1) }}%</span>

            <BaseBadge :variant="GOAL_STATUS_VARIANT[goal.status] ?? 'neutral'" size="sm">
              {{ serverLabel('goalStatus', goal.status) }}
            </BaseBadge>
          </li>
        </ul>
      </BaseCard>

      <p class="generated">
        {{ t('composed.generatedNote', {
          when: formatDateTime(report.generatedAt),
          currency: report.currency
        }) }}
      </p>
    </motion.div>
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/reports/ReportsView.scss"></style>
