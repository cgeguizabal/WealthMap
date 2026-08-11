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
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

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
  { value: 'active', label: t('common.active'), count: activePlans.value.length },
  { value: 'completed', label: t('common.completed'), count: completedPlans.value.length }
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
      :title="t('installments.title')"
      :subtitle="t('installments.subtitle')"
    >
      <template #actions>
        <BaseButton variant="primary" @click="formOpen = true">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          {{ t('installments.newPlan') }}
        </BaseButton>
      </template>
    </PageHeader>

    <div v-if="outstanding.length" class="totals">
      <div v-for="entry in outstanding" :key="entry.currency" class="totals__item">
        <span class="totals__label">{{ t('installments.stillToPay') }}</span>
        <span class="totals__value numeric">{{ format(entry.total, { currency: entry.currency }) }}</span>
      </div>
    </div>

    <CardGridSkeleton v-if="loading && !plans?.length" />

    <BaseEmptyState
      v-else-if="error"
      icon="alert"
      :title="t('installments.loadFailed')"
      :message="error.message"
    >
      <template #action><BaseButton variant="primary" @click="loadPlans">{{ t('common.tryAgain') }}</BaseButton></template>
    </BaseEmptyState>

    <BaseEmptyState
      v-else-if="!plans?.length"
      icon="layers"
      :title="t('installments.emptyTitle')"
      :message="t('installments.emptyMessage')"
    >
      <template #action><BaseButton variant="primary" @click="formOpen = true">{{ t('installments.addFirst') }}</BaseButton></template>
    </BaseEmptyState>

    <template v-else>
      <BaseTabs v-model="tab" :tabs="tabs" class="tabs" />

      <BaseEmptyState
        v-if="!visible.length"
        :icon="tab === 'active' ? 'check-circle' : 'layers'"
        :title="tab === 'active' ? t('installments.nothingOutstanding') : t('installments.nothingCompleted')"
        :message="tab === 'active' ? t('installments.allPaidOff') : t('installments.completedHint')"
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

<style scoped lang="scss" src="./InstallmentsView.scss"></style>
