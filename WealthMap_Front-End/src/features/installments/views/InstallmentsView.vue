<script setup>
import { ref, computed, onMounted } from 'vue'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { installmentsApi } from '@/api/installments.api'
import { useAsync } from '@/composables/useAsync'
import { useMoney } from '@/composables/useMoney'
import { useDashboardStore } from '@/stores/dashboard.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseTabs from '@/components/base/BaseTabs.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'
import CardGridSkeleton from '@/features/shared/components/CardGridSkeleton.vue'

import PlanCard from '../components/PlanCard.vue'
import PlanFormModal from '../components/PlanFormModal.vue'
import PayInstallmentModal from '../components/PayInstallmentModal.vue'

const { format } = useMoney()
const dashboard = useDashboardStore()

const { data: plans, loading, error, run: loadPlans } = useAsync(installmentsApi.list, { initialData: [] })

const formOpen = ref(false)
const payOpen = ref(false)
const active = ref(null)
const tab = ref('active')

const activePlans = computed(() => (plans.value ?? []).filter((p) => !p.isCompleted))
const completedPlans = computed(() => (plans.value ?? []).filter((p) => p.isCompleted))

const tabs = computed(() => [
  { value: 'active', label: 'Active', count: activePlans.value.length },
  { value: 'completed', label: 'Completed', count: completedPlans.value.length }
])

const visible = computed(() => (tab.value === 'active' ? activePlans.value : completedPlans.value))

const outstanding = computed(() => {
  const byCurrency = new Map()

  for (const plan of activePlans.value) {
    byCurrency.set(plan.currency, (byCurrency.get(plan.currency) ?? 0) + plan.remainingBalance)
  }

  return [...byCurrency.entries()].map(([currency, total]) => ({ currency, total }))
})

function openPay(plan) {
  active.value = plan
  payOpen.value = true
}

function refresh() {
  loadPlans()
  dashboard.invalidate()
}

onMounted(loadPlans)
</script>

<template>
  <div>
    <PageHeader
      title="Installments"
      subtitle="Interest-free plans. The full price is charged to the card up front, then repaid month by month."
    >
      <template #actions>
        <BaseButton variant="primary" @click="formOpen = true">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          New plan
        </BaseButton>
      </template>
    </PageHeader>

    <div v-if="outstanding.length" class="totals">
      <div v-for="entry in outstanding" :key="entry.currency" class="totals__item">
        <span class="totals__label">Still to pay</span>
        <span class="totals__value numeric">{{ format(entry.total, { currency: entry.currency }) }}</span>
      </div>
    </div>

    <CardGridSkeleton v-if="loading && !plans?.length" />

    <BaseEmptyState
      v-else-if="error"
      icon="alert"
      title="Could not load your plans"
      :message="error.message"
    >
      <template #action><BaseButton variant="primary" @click="loadPlans">Try again</BaseButton></template>
    </BaseEmptyState>

    <BaseEmptyState
      v-else-if="!plans?.length"
      icon="layers"
      title="No installment plans"
      message="Split a purchase across months at no interest. The card is charged in full today."
    >
      <template #action><BaseButton variant="primary" @click="formOpen = true">Create a plan</BaseButton></template>
    </BaseEmptyState>

    <template v-else>
      <BaseTabs v-model="tab" :tabs="tabs" class="tabs" />

      <BaseEmptyState
        v-if="!visible.length"
        :icon="tab === 'active' ? 'check-circle' : 'layers'"
        :title="tab === 'active' ? 'Nothing outstanding' : 'Nothing completed yet'"
        :message="tab === 'active' ? 'Every plan is paid off.' : 'Plans appear here once the last installment is paid.'"
        compact
      />

      <motion.div
        v-else
        class="grid"
      v-bind="fadeUp()"
      >
        <PlanCard v-for="plan in visible" :key="plan.id" :plan="plan" @pay="openPay" />
      </motion.div>
    </template>

    <PlanFormModal v-model="formOpen" @saved="refresh" />
    <PayInstallmentModal v-model="payOpen" :plan="active" @saved="refresh" />
  </div>
</template>

<style scoped lang="scss">
.totals {
  display: flex;
  flex-wrap: wrap;
  gap: var(--sp-3);
  margin-bottom: var(--sp-5);
}

.totals__item {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: var(--sp-3) var(--sp-4);
  background: var(--canvas-alt);
  border: var(--border-subtle);
  border-radius: var(--radius);
}

.totals__label {
  font-size: var(--fs-xs);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-muted);
}

.totals__value { font-size: var(--fs-lg); font-weight: var(--fw-semibold); }

.tabs { margin-bottom: var(--sp-4); }

.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: var(--sp-4);
}


@media (max-width: 640px) {
  .grid { grid-template-columns: 1fr; }
}
</style>
