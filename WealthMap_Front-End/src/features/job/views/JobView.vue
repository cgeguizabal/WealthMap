<script setup>
import { ref, computed, onMounted } from 'vue'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { jobsApi } from '@/api/jobs.api'
import { incomesApi, toMonthly } from '@/api/incomes.api'
import { freelanceJobsApi, FREELANCE_STATUS_VARIANT } from '@/api/freelanceJobs.api'
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
import BaseTabs from '@/components/base/BaseTabs.vue'

import JobFormModal from '../components/JobFormModal.vue'
import DeductionFormModal from '../components/DeductionFormModal.vue'
import IncomeFormModal from '../components/IncomeFormModal.vue'
import FreelanceFormModal from '../components/FreelanceFormModal.vue'
import FreelancePaymentModal from '../components/FreelancePaymentModal.vue'
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
const freelanceJobs = ref([])
const loading = ref(false)

const jobOpen = ref(false)
const deductionOpen = ref(false)
const incomeOpen = ref(false)
const freelanceOpen = ref(false)
const freelancePaymentOpen = ref(false)
const editingFreelance = ref(null)
const payingFreelance = ref(null)
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
    const [jobResult, incomeResult, freelanceResult] = await Promise.allSettled([
      jobsApi.list(),
      incomesApi.list(),
      freelanceJobsApi.list()
    ])

    // One job per user, so the list holds at most one.
    job.value = jobResult.status === 'fulfilled' ? (jobResult.value[0] ?? null) : null
    incomes.value = incomeResult.status === 'fulfilled' ? incomeResult.value : []
    freelanceJobs.value = freelanceResult.status === 'fulfilled' ? freelanceResult.value : []
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

/**
 * What clients still owe. Shown for awareness only — it is deliberately absent
 * from every figure that feeds "safe to spend", because unpaid work is not money.
 * Once paid it stops being counted here and becomes an ordinary account balance,
 * which is where it starts affecting what can be spent.
 */
const totalOutstanding = computed(() =>
  freelanceJobs.value.reduce((sum, work) => sum + work.outstanding, 0)
)

/**
 * Work still in play, and work that is finished with.
 *
 * The list is sorted with unfinished first, so the top stayed useful without
 * this — but after a year of freelancing you would scroll past dozens of paid
 * jobs to reach the current ones. Splitting them keeps the default view to what
 * still needs doing without hiding anything.
 */
const freelanceTab = ref('active')

const activeFreelance = computed(() =>
  freelanceJobs.value.filter((w) => w.status === 'InProgress' || w.status === 'Delivered')
)

const finishedFreelance = computed(() =>
  freelanceJobs.value.filter((w) => w.status === 'Paid' || w.status === 'Cancelled')
)

const visibleFreelance = computed(() =>
  freelanceTab.value === 'active' ? activeFreelance.value : finishedFreelance.value
)

const freelanceTabs = computed(() => [
  { value: 'active', label: t('freelance.active'), count: activeFreelance.value.length },
  { value: 'history', label: t('freelance.history'), count: finishedFreelance.value.length }
])

async function loadFreelance() {
  freelanceJobs.value = await freelanceJobsApi.list()
}

function openFreelance(work = null) {
  editingFreelance.value = work
  freelanceOpen.value = true
}

function openPayment(work) {
  payingFreelance.value = work
  freelancePaymentOpen.value = true
}

async function markDelivered(work) {
  try {
    await freelanceJobsApi.markDelivered(work.id, new Date().toISOString().slice(0, 10))
    toast.success(t('freelance.markedDelivered'))
    await loadFreelance()
  } catch (err) {
    toast.error(err.message)
  }
}

async function removeFreelance(work) {
  // Paid work moved money, so removing it moves it back. That is worth spelling
  // out before the click rather than surprising someone with a changed balance.
  const confirmed = await ui.confirm({
    title: t('freelance.deleteTitle'),
    message: work.status === 'Paid'
      ? t('freelance.deletePaidMessage', { title: work.title })
      : t('freelance.deleteMessage', { title: work.title }),
    confirmLabel: t('common.delete'),
    variant: 'danger'
  })

  if (!confirmed) return

  try {
    await freelanceJobsApi.remove(work.id)
    toast.success(t('freelance.deleted'))
    await loadFreelance()
    if (work.status === 'Paid') dashboard.load()
  } catch (err) {
    toast.error(err.message)
  }
}
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


      <!-- ── Freelance work ──────────────────── -->
      <BaseCard
        data-tour="job-freelance"
        :title="t('freelance.title')"
        :subtitle="t('freelance.subtitle')"
        :padded="false"
        class="incomes-card"
      >
        <template #actions>
          <BaseButton size="sm" variant="secondary" @click="openFreelance()">
            <template #icon><BaseIcon name="plus" :size="14" /></template>
            {{ t('common.add') }}
          </BaseButton>
        </template>

        <BaseEmptyState
          v-if="!freelanceJobs.length"
          icon="briefcase"
          :title="t('freelance.emptyTitle')"
          :message="t('freelance.emptyMessage')"
          compact
        />

        <template v-else>
          <BaseTabs v-model="freelanceTab" :tabs="freelanceTabs" class="freelance-tabs" />

          <BaseEmptyState
            v-if="!visibleFreelance.length"
            icon="info"
            :title="freelanceTab === 'active'
              ? t('freelance.noneActiveTitle')
              : t('freelance.noneFinishedTitle')"
            :message="freelanceTab === 'active'
              ? t('freelance.noneActiveMessage')
              : t('freelance.noneFinishedMessage')"
            compact
          />

          <ul v-else class="incomes">
            <li v-for="work in visibleFreelance" :key="work.id" class="income">
            <div class="income__main">
              <span class="income__name">{{ work.title }}</span>

              <span class="income__meta">
                <BaseBadge size="sm" :variant="FREELANCE_STATUS_VARIANT[work.status]">
                  {{ t(`freelance.status.${work.status}`) }}
                </BaseBadge>

                <template v-if="work.client"> · {{ work.client }}</template>
                <template v-if="work.status === 'Paid' && work.paidOn">
                  · {{ t('freelance.paidOnDate', { date: work.paidOn }) }}
                </template>
                <template v-else-if="work.dueOn">
                  · {{ t('freelance.dueBy', { date: work.dueOn }) }}
                </template>
              </span>
            </div>

            <span class="income__amount numeric">
              {{ format(work.status === 'Paid' ? work.amountPaid : work.agreedAmount,
                        { currency: work.currency }) }}
            </span>

            <div class="income__actions">
              <!-- Delivered is a fact about the work, not about money, so it is
                   offered on its own before any payment exists. -->
              <BaseButton
                v-if="work.status === 'InProgress'"
                size="sm"
                variant="ghost"
                :title="t('freelance.markDelivered')"
                @click="markDelivered(work)"
              >
                <template #icon><BaseIcon name="check" :size="14" /></template>
                <span class="sr-only">{{ t('freelance.markDelivered') }}</span>
              </BaseButton>

              <BaseButton
                v-if="work.status === 'InProgress' || work.status === 'Delivered'"
                size="sm"
                variant="secondary"
                @click="openPayment(work)"
              >
                {{ t('freelance.gotPaid') }}
              </BaseButton>

              <BaseButton
                v-if="work.status !== 'Paid' && work.status !== 'Cancelled'"
                size="sm"
                variant="ghost"
                :title="t('common.edit')"
                @click="openFreelance(work)"
              >
                <template #icon><BaseIcon name="pencil" :size="14" /></template>
                <span class="sr-only">{{ t('common.edit') }}</span>
              </BaseButton>

              <BaseButton
                size="sm"
                variant="ghost"
                :title="t('common.remove')"
                @click="removeFreelance(work)"
              >
                <template #icon><BaseIcon name="trash" :size="14" /></template>
                <span class="sr-only">{{ t('common.remove') }}</span>
              </BaseButton>
            </div>
          </li>
          </ul>
        </template>

        <!--
          Outstanding is shown, and deliberately kept out of every total that
          feeds "safe to spend". Work that has not been paid for is a hope with a
          name on it; treating it as money would be the one place this app told
          you to spend what may never arrive.
        -->
        <template v-if="freelanceJobs.length" #footer>
          <div class="incomes__footer">
            <span>{{ t('freelance.outstandingLabel') }}</span>
            <span class="numeric">{{ format(totalOutstanding) }}</span>
          </div>
        </template>
      </BaseCard>

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

    <FreelanceFormModal
      v-model="freelanceOpen"
      :job="editingFreelance"
      @saved="loadFreelance"
    />

    <FreelancePaymentModal
      v-model="freelancePaymentOpen"
      :job="payingFreelance"
      @saved="loadFreelance"
    />
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/job/JobView.scss"></style>
