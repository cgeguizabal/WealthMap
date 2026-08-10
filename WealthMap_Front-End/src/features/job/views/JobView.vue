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

const { format } = useMoney()
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
    title: `Remove ${deduction.name}?`,
    message: 'Your net salary goes back up by this amount.',
    confirmLabel: 'Remove',
    variant: 'danger'
  })

  if (!confirmed) return

  try {
    job.value = await jobsApi.removeDeduction(job.value.id, deduction.id)
    toast.success('Deduction removed.')
    dashboard.invalidate()
  } catch (err) {
    toast.error(err.message)
  }
}

async function removeJob() {
  const confirmed = await ui.confirm({
    title: `Delete ${job.value.title}?`,
    message: 'The job and all its deductions are removed. Your accounts are untouched.',
    confirmLabel: 'Delete job',
    variant: 'danger'
  })

  if (!confirmed) return

  try {
    await jobsApi.remove(job.value.id)
    toast.success('Job deleted.')
    job.value = null
    dashboard.invalidate()
  } catch (err) {
    toast.error(err.message)
  }
}

async function removeIncome(income) {
  const confirmed = await ui.confirm({
    title: `Remove ${income.name}?`,
    confirmLabel: 'Remove',
    variant: 'danger'
  })

  if (!confirmed) return

  try {
    await incomesApi.remove(income.id)
    toast.success('Income removed.')
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
      title="Job & income"
      subtitle="Your salary, what comes out of it, and anything else that arrives regularly."
    >
      <template #actions>
        <BaseButton variant="secondary" @click="openIncome()">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          Add income
        </BaseButton>

        <BaseButton v-if="!job" variant="primary" @click="jobOpen = true">
          <template #icon><BaseIcon name="briefcase" :size="15" /></template>
          Add job
        </BaseButton>
      </template>
    </PageHeader>

    <div v-if="loading && !job && !incomes.length" class="state"><BaseSpinner :size="22" /></div>

    <template v-else>
      <!-- ── Job ─────────────────────────────── -->
      <BaseEmptyState
        v-if="!job"
        icon="briefcase"
        title="No job recorded"
        message="Add your salary and its deductions, and WealthMap works out your real take-home and when it lands."
        class="empty"
      >
        <template #action>
          <BaseButton variant="primary" @click="jobOpen = true">Add your job</BaseButton>
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
                Edit
              </BaseButton>
              <BaseButton size="sm" variant="ghost" @click="removeJob">
                <template #icon><BaseIcon name="trash" :size="14" /></template>
                <span class="sr-only">Delete job</span>
              </BaseButton>
            </div>
          </header>

          <div class="job__figures">
            <div class="job__figure">
              <span class="job__label">Gross monthly</span>
              <p class="numeric">{{ format(job.grossMonthlySalary, { currency: job.currency }) }}</p>
            </div>

            <div class="job__figure">
              <span class="job__label">Deducted</span>
              <p class="numeric is-negative">−{{ format(totalDeducted, { currency: job.currency }) }}</p>
            </div>

            <div class="job__figure job__figure--headline">
              <span class="job__label">Net monthly</span>
              <p class="numeric">{{ format(job.netMonthly, { currency: job.currency }) }}</p>
            </div>

            <div class="job__figure">
              <span class="job__label">Per deposit</span>
              <p class="numeric">{{ format(job.netPerDeposit, { currency: job.currency }) }}</p>
            </div>
          </div>

          <div class="job__schedule">
            <span class="job__label">Paid on day</span>
            <div class="job__days">
              <BaseBadge v-for="day in job.paymentDays" :key="day" size="sm">{{ day }}</BaseBadge>
            </div>

            <span class="job__next">
              Next:
              <template v-for="(date, index) in job.nextPaymentDates" :key="date">
                {{ date }}<span v-if="index < job.nextPaymentDates.length - 1"> · </span>
              </template>
            </span>
          </div>
        </div>

        <BaseCard title="Deductions" subtitle="Taken from your payslip — the app does the arithmetic, not the tax law" :padded="false">
          <template #actions>
            <BaseButton size="sm" variant="secondary" @click="openDeduction()">
              <template #icon><BaseIcon name="plus" :size="14" /></template>
              Add
            </BaseButton>
          </template>

          <BaseEmptyState
            v-if="!job.deductions.length"
            icon="minus"
            title="No deductions"
            message="Net equals gross until you add what comes out."
            compact
          />

          <ul v-else class="deductions">
            <li v-for="deduction in job.deductions" :key="deduction.id" class="deduction">
              <div class="deduction__body">
                <span class="deduction__name">{{ deduction.name }}</span>
                <span class="deduction__type">
                  {{ deduction.type === 'Percentage' ? 'Percentage of gross' : 'Fixed amount' }}
                </span>
              </div>

              <span class="deduction__value numeric">
                {{ deduction.type === 'Percentage'
                  ? `${deduction.value}%`
                  : format(deduction.value, { currency: job.currency }) }}
              </span>

              <div class="deduction__actions">
                <BaseButton size="sm" variant="ghost" title="Edit" @click="openDeduction(deduction)">
                  <template #icon><BaseIcon name="pencil" :size="14" /></template>
                  <span class="sr-only">Edit</span>
                </BaseButton>
                <BaseButton size="sm" variant="ghost" title="Remove" @click="removeDeduction(deduction)">
                  <template #icon><BaseIcon name="trash" :size="14" /></template>
                  <span class="sr-only">Remove</span>
                </BaseButton>
              </div>
            </li>
          </ul>
        </BaseCard>
      </motion.div>

      <!-- ── Additional incomes ──────────────── -->
      <BaseCard
        title="Other income"
        subtitle="Recurring extras — one-off money is a bonus deposit on an account"
        :padded="false"
        class="incomes-card"
      >
        <template #actions>
          <BaseButton size="sm" variant="secondary" @click="openIncome()">
            <template #icon><BaseIcon name="plus" :size="14" /></template>
            Add
          </BaseButton>
        </template>

        <BaseEmptyState
          v-if="!incomes.length"
          icon="plus"
          title="No other income"
          message="Freelance work, rent, anything that arrives on a schedule."
          compact
        />

        <ul v-else class="incomes">
          <li v-for="income in incomes" :key="income.id" class="income">
            <div class="income__body">
              <span class="income__name">{{ income.name }}</span>
              <span class="income__meta">
                {{ income.frequency }} ·
                {{ format(toMonthly(income.amount, income.frequency), { currency: income.currency }) }}/mo
              </span>
            </div>

            <span class="income__amount numeric">
              {{ format(income.amount, { currency: income.currency }) }}
            </span>

            <div class="income__actions">
              <BaseButton size="sm" variant="ghost" title="Edit" @click="openIncome(income)">
                <template #icon><BaseIcon name="pencil" :size="14" /></template>
                <span class="sr-only">Edit</span>
              </BaseButton>
              <BaseButton size="sm" variant="ghost" title="Remove" @click="removeIncome(income)">
                <template #icon><BaseIcon name="trash" :size="14" /></template>
                <span class="sr-only">Remove</span>
              </BaseButton>
            </div>
          </li>
        </ul>

        <template v-if="incomes.length" #footer>
          <div class="incomes__footer">
            <span>Monthly equivalent</span>
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

<style scoped lang="scss">
.job { display: flex; flex-direction: column; gap: var(--sp-4); margin-bottom: var(--sp-5); }

.job__summary {
  display: flex;
  flex-direction: column;
  gap: var(--sp-5);

  padding: var(--sp-5);
  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
}

.job__head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--sp-4);
}

.job__title { font-size: var(--fs-lg); font-weight: var(--fw-semibold); }
.job__employer { font-size: var(--fs-sm); color: var(--text-muted); }
.job__head-actions { display: flex; gap: var(--sp-1); flex: none; }

.job__figures {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--sp-4);
  padding-top: var(--sp-4);
  border-top: var(--border-subtle);
}

.job__label {
  font-size: var(--fs-xs);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-muted);
}

.job__figure p { font-size: var(--fs-md); font-weight: var(--fw-medium); margin-top: 2px; }
.job__figure .is-negative { color: var(--negative); }

.job__figure--headline p {
  font-size: var(--fs-xl);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.02em;
}

.job__schedule {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: var(--sp-3);
  padding-top: var(--sp-4);
  border-top: var(--border-subtle);
}

.job__days { display: flex; gap: var(--sp-1); }
.job__next { font-size: var(--fs-xs); color: var(--text-muted); }

.deductions, .incomes { display: flex; flex-direction: column; }

.deduction, .income {
  display: flex;
  align-items: center;
  gap: var(--sp-3);
  padding: var(--sp-3) var(--sp-5);
  border-bottom: var(--border-subtle);

  &:last-child { border-bottom: none; }
}

.deduction__body, .income__body { flex: 1; display: flex; flex-direction: column; min-width: 0; }
.deduction__name, .income__name { font-weight: var(--fw-medium); }
.deduction__type, .income__meta { font-size: var(--fs-xs); color: var(--text-muted); }
.deduction__value, .income__amount { font-weight: var(--fw-semibold); }
.deduction__actions, .income__actions { display: flex; gap: var(--sp-1); flex: none; }

.incomes-card { margin-bottom: var(--sp-5); }

.incomes__footer {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  font-size: var(--fs-sm);
  color: var(--text-muted);

  .numeric { font-size: var(--fs-md); font-weight: var(--fw-semibold); color: var(--text); }
}

.empty { margin-bottom: var(--sp-5); }
.state { display: grid; place-items: center; padding: var(--sp-12); color: var(--text-muted); }

@media (max-width: 900px) {
  .job__figures { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 767px) {
  .deduction, .income { padding: var(--sp-3) var(--sp-4); }
}

@media (max-width: 480px) {
  .job__figures { grid-template-columns: 1fr; }
}
</style>
