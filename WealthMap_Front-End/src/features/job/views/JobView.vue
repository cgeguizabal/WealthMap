<script setup>
import { ref, computed, onMounted } from 'vue'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { jobsApi } from '@/api/jobs.api'
import { incomesApi, toMonthly } from '@/api/incomes.api'
import { useMoney } from '@/composables/useMoney'
import { useToast } from '@/composables/useToast'
import { useUiStore } from '@/stores/ui.store'
import { useDashboardStore } from '@/stores/dashboard.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseSpinner from '@/components/base/BaseSpinner.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'

import JobFormModal from '../components/JobFormModal.vue'
import DeductionFormModal from '../components/DeductionFormModal.vue'
import IncomeFormModal from '../components/IncomeFormModal.vue'
import { useI18n } from '@/composables/useI18n'
import { useServerText } from '@/composables/useServerText'

const { t } = useI18n()
const { label: serverLabel } = useServerText()

const { format, roundCents } = useMoney()
const toast = useToast()
const ui = useUiStore()
const dashboard = useDashboardStore()

const job = ref(null)
const incomes = ref([])
const loading = ref(false)

const jobOpen = ref(false)
const deductionOpen = ref(false)
const incomeOpen = ref(false)
const editingDeduction = ref(null)
const editingIncome = ref(null)

const totalDeducted = computed(() => {
  if (!job.value) return 0
  return job.value.grossMonthlySalary - job.value.netMonthly
})

const monthlyExtras = computed(() =>
  incomes.value.reduce((sum, income) => sum + toMonthly(income.amount, income.frequency), 0)
)

async function load() {
  loading.value = true

  try {
    const [jobResult, incomeResult] = await Promise.allSettled([
      jobsApi.list(),
      incomesApi.list()
    ])

    // One job per user, so the list holds at most one.
    job.value = jobResult.status === 'fulfilled' ? (jobResult.value[0] ?? null) : null
    incomes.value = incomeResult.status === 'fulfilled' ? incomeResult.value : []
  } finally {
    loading.value = false
  }
}

/** Deduction endpoints answer with the whole job, so state updates without a refetch. */
function onJobMutated(updated) {
  job.value = updated
  dashboard.invalidate()
}

/** What this deduction costs over the whole month, whatever type it is. */
function monthlyCost(deduction) {
  if (!job.value) return 0

  return roundCents(
    deduction.type === 'Percentage'
      ? (job.value.grossMonthlySalary * Number(deduction.value)) / 100
      : Number(deduction.value)
  )
}

/** Deductions are monthly; each payday carries an equal share of them. */
function perPaydayCost(deduction) {
  const days = job.value?.paymentDays?.length || 1
  return roundCents(monthlyCost(deduction) / days)
}

function openDeduction(deduction = null) {
  editingDeduction.value = deduction
  deductionOpen.value = true
}

function openIncome(income = null) {
  editingIncome.value = income
  incomeOpen.value = true
}

async function removeDeduction(deduction) {
  const confirmed = await ui.confirm({
    title: t('job.removeDeductionTitle', { name: deduction.name }),
    message: t('job.removeDeductionMessage'),
    confirmLabel: t('common.remove'),
    variant: 'danger'
  })

  if (!confirmed) return

  try {
    job.value = await jobsApi.removeDeduction(job.value.id, deduction.id)
    toast.success(t('job.deductionRemoved'))
    dashboard.invalidate()
  } catch (err) {
    toast.error(err.message)
  }
}

async function removeJob() {
  const confirmed = await ui.confirm({
    title: t('job.deleteJobTitle', { name: job.value.title }),
    message: t('job.deleteJobMessage'),
    confirmLabel: t('job.deleteJob'),
    variant: 'danger'
  })

  if (!confirmed) return

  try {
    await jobsApi.remove(job.value.id)
    toast.success(t('job.jobDeleted'))
    job.value = null
    dashboard.invalidate()
  } catch (err) {
    toast.error(err.message)
  }
}

async function removeIncome(income) {
  const confirmed = await ui.confirm({
    title: t('job.removeIncomeTitle', { name: income.name }),
    confirmLabel: t('common.remove'),
    variant: 'danger'
  })

  if (!confirmed) return

  try {
    await incomesApi.remove(income.id)
    toast.success(t('job.incomeRemoved'))
    load()
    dashboard.invalidate()
  } catch (err) {
    toast.error(err.message)
  }
}

function onIncomeSaved() {
  load()
  dashboard.invalidate()
}

onMounted(load)
</script>

<template>
  <div>
    <PageHeader
      :title="t('job.title')"
      :subtitle="t('job.subtitle')"
    >
      <template #actions>
        <BaseButton data-tour="job-income" variant="secondary" @click="openIncome()">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          {{ t('job.addIncome') }}
        </BaseButton>

        <BaseButton v-if="!job" variant="primary" @click="jobOpen = true">
          <template #icon><BaseIcon name="briefcase" :size="15" /></template>
          {{ t('job.addJob') }}
        </BaseButton>
      </template>
    </PageHeader>

    <div v-if="loading && !job && !incomes.length" class="state"><BaseSpinner :size="22" /></div>

    <template v-else>
      <!-- ── Job ─────────────────────────────── -->
      <BaseEmptyState
        v-if="!job"
        icon="briefcase"
        :title="t('job.emptyTitle')"
        :message="t('job.emptyMessage')"
        class="empty"
      >
        <template #action>
          <BaseButton variant="primary" @click="jobOpen = true">{{ t('job.addJob') }}</BaseButton>
        </template>
      </BaseEmptyState>

      <motion.div
        v-else
        class="job"
      v-bind="fadeUp()"
      >
        <div class="job__summary">
          <header class="job__head">
            <div>
              <h2 class="job__title">{{ job.title }}</h2>
              <p class="job__employer">{{ job.employer }}</p>
            </div>

            <div class="job__head-actions">
              <BaseButton size="sm" variant="ghost" @click="jobOpen = true">
                <template #icon><BaseIcon name="pencil" :size="14" /></template>
                {{ t('common.edit') }}
              </BaseButton>
              <BaseButton size="sm" variant="ghost" @click="removeJob">
                <template #icon><BaseIcon name="trash" :size="14" /></template>
                <span class="sr-only">{{ t('job.deleteJob') }}</span>
              </BaseButton>
            </div>
          </header>

          <div class="job__figures" data-tour="job-salary">
            <div class="job__figure">
              <span class="job__label">{{ t('job.grossMonthly') }}</span>
              <p class="numeric">{{ format(job.grossMonthlySalary, { currency: job.currency }) }}</p>
            </div>

            <div class="job__figure">
              <span class="job__label">{{ t('job.deducted') }}</span>
              <p class="numeric is-negative">−{{ format(totalDeducted, { currency: job.currency }) }}</p>
            </div>

            <div class="job__figure job__figure--headline">
              <span class="job__label">{{ t('job.netMonthly') }}</span>
              <p class="numeric">{{ format(job.netMonthly, { currency: job.currency }) }}</p>
            </div>

            <div class="job__figure">
              <span class="job__label">{{ t('job.perDeposit') }}</span>
              <p class="numeric">{{ format(job.netPerDeposit, { currency: job.currency }) }}</p>
            </div>
          </div>

          <div class="job__schedule" data-tour="job-days">
            <span class="job__label">{{ t('job.paidOnDay') }}</span>
            <div class="job__days">
              <BaseBadge v-for="day in job.paymentDays" :key="day" size="sm">{{ day }}</BaseBadge>
            </div>

            <span class="job__next">
              {{ t('job.nextPayday') }}
              <template v-for="(date, index) in job.nextPaymentDates" :key="date">
                {{ date }}<span v-if="index < job.nextPaymentDates.length - 1"> · </span>
              </template>
            </span>
          </div>
        </div>

        <BaseCard data-tour="job-deductions" :title="t('job.deductions')" :subtitle="t('job.deductionsSubtitle')" :padded="false">
          <template #actions>
            <BaseButton size="sm" variant="secondary" @click="openDeduction()">
              <template #icon><BaseIcon name="plus" :size="14" /></template>
              {{ t('common.add') }}
            </BaseButton>
          </template>

          <BaseEmptyState
            v-if="!job.deductions.length"
            icon="minus"
            :title="t('job.noDeductionsTitle')"
            :message="t('job.noDeductionsMessage')"
            compact
          />

          <ul v-else class="deductions">
            <li v-for="deduction in job.deductions" :key="deduction.id" class="deduction">
              <div class="deduction__body">
                <span class="deduction__name">{{ deduction.name }}</span>
                <span class="deduction__type">
                  {{ serverLabel('deductionType', deduction.type) }}
                </span>
                <!--
                  Spelling out the monthly total and the per-payday share, because a
                  rate alone leaves it ambiguous whether it is charged once a month
                  or again at every payday. It is once a month, split evenly.
                -->
                <span class="deduction__split">
                  {{ t('job.perMonth', { amount: format(monthlyCost(deduction), { currency: job.currency }) }) }}
                  <template v-if="job.paymentDays.length > 1">
                    · {{ t('job.perPayday', { amount: format(perPaydayCost(deduction), { currency: job.currency }), count: job.paymentDays.length }) }}
                  </template>
                </span>
              </div>

              <span class="deduction__value numeric">
                {{ deduction.type === 'Percentage'
                  ? `${deduction.value}%`
                  : format(deduction.value, { currency: job.currency }) }}
              </span>

              <div class="deduction__actions">
                <BaseButton size="sm" variant="ghost" :title="t('common.edit')" @click="openDeduction(deduction)">
                  <template #icon><BaseIcon name="pencil" :size="14" /></template>
                  <span class="sr-only">{{ t('common.edit') }}</span>
                </BaseButton>
                <BaseButton size="sm" variant="ghost" :title="t('common.remove')" @click="removeDeduction(deduction)">
                  <template #icon><BaseIcon name="trash" :size="14" /></template>
                  <span class="sr-only">{{ t('common.remove') }}</span>
                </BaseButton>
              </div>
            </li>
          </ul>
        </BaseCard>
      </motion.div>

      <!-- ── Additional incomes ──────────────── -->
      <BaseCard
        :title="t('job.otherIncome')"
        :subtitle="t('job.otherIncomeSubtitle')"
        :padded="false"
        class="incomes-card"
      >
        <template #actions>
          <BaseButton size="sm" variant="secondary" @click="openIncome()">
            <template #icon><BaseIcon name="plus" :size="14" /></template>
            {{ t('common.add') }}
          </BaseButton>
        </template>

        <BaseEmptyState
          v-if="!incomes.length"
          icon="plus"
          :title="t('job.noOtherIncome')"
          :message="t('job.noOtherIncomeMessage')"
          compact
        />

        <ul v-else class="incomes">
          <li v-for="income in incomes" :key="income.id" class="income">
            <div class="income__body">
              <span class="income__name">{{ income.name }}</span>
              <span class="income__meta">
                {{ serverLabel('incomeFrequency', income.frequency) }} ·
                {{ format(toMonthly(income.amount, income.frequency), { currency: income.currency }) }}/mo
              </span>
            </div>

            <span class="income__amount numeric">
              {{ format(income.amount, { currency: income.currency }) }}
            </span>

            <div class="income__actions">
              <BaseButton size="sm" variant="ghost" :title="t('common.edit')" @click="openIncome(income)">
                <template #icon><BaseIcon name="pencil" :size="14" /></template>
                <span class="sr-only">{{ t('common.edit') }}</span>
              </BaseButton>
              <BaseButton size="sm" variant="ghost" :title="t('common.remove')" @click="removeIncome(income)">
                <template #icon><BaseIcon name="trash" :size="14" /></template>
                <span class="sr-only">{{ t('common.remove') }}</span>
              </BaseButton>
            </div>
          </li>
        </ul>

        <template v-if="incomes.length" #footer>
          <div class="incomes__footer">
            <span>{{ t('job.monthlyEquivalent') }}</span>
            <span class="numeric">{{ format(monthlyExtras) }}</span>
          </div>
        </template>
      </BaseCard>
    </template>

    <JobFormModal v-model="jobOpen" :job="job" @saved="onJobMutated" />

    <DeductionFormModal
      v-if="job"
      v-model="deductionOpen"
      :job="job"
      :deduction="editingDeduction"
      @saved="onJobMutated"
    />

    <IncomeFormModal v-model="incomeOpen" :income="editingIncome" @saved="onIncomeSaved" />
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/job/JobView.scss"></style>
