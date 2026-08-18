<script setup>
import { ref, computed, onMounted } from 'vue'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { savingsGoalsApi, productGoalsApi } from '@/api/goals.api'
import { useMoney } from '@/composables/useMoney'
import { useToast } from '@/composables/useToast'
import { useUiStore } from '@/stores/ui.store'
import { useDashboardStore } from '@/stores/dashboard.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseTabs from '@/components/base/BaseTabs.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'
import CardGridSkeleton from '@/features/shared/components/CardGridSkeleton.vue'

import GoalCard from '../components/GoalCard.vue'
import GoalFormModal from '../components/GoalFormModal.vue'
import ContributeModal from '../components/ContributeModal.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const { format } = useMoney()
const toast = useToast()
const ui = useUiStore()
const dashboard = useDashboardStore()

const savings = ref([])
const products = ref([])
const loading = ref(false)

const tab = ref('savings')
const formOpen = ref(false)
const contributeOpen = ref(false)
const editing = ref(null)
const active = ref(null)

const isSavingsTab = computed(() => tab.value === 'savings')
const visible = computed(() => (isSavingsTab.value ? savings.value : products.value))

const tabs = computed(() => [
  { value: 'savings', label: t('goals.savings'), count: savings.value.length },
  { value: 'product', label: t('goals.products'), count: products.value.length }
])

const summary = computed(() => {
  const all = [...savings.value, ...products.value]
  const behind = all.filter((g) => g.status === 'BehindSchedule' || g.status === 'DeadlinePassed').length
  const done = all.filter((g) => g.status === 'Completed').length

  return { total: all.length, behind, done }
})

async function load() {
  loading.value = true

  try {
    const [savingsResult, productsResult] = await Promise.allSettled([
      savingsGoalsApi.list(),
      productGoalsApi.list()
    ])

    savings.value = savingsResult.status === 'fulfilled' ? savingsResult.value : []
    products.value = productsResult.status === 'fulfilled' ? productsResult.value : []
  } finally {
    loading.value = false
  }
}

function openCreate() {
  editing.value = null
  formOpen.value = true
}

function openEdit(goal) {
  editing.value = goal
  formOpen.value = true
}

function openContribute(goal) {
  active.value = goal
  contributeOpen.value = true
}

async function remove(goal) {
  const confirmed = await ui.confirm({
    title: t('goals.deleteTitle', { name: goal.name }),
    message: t('goals.deleteMessage'),
    confirmLabel: t('common.delete'),
    variant: 'danger'
  })

  if (!confirmed) return

  try {
    await (isSavingsTab.value ? savingsGoalsApi.remove(goal.id) : productGoalsApi.remove(goal.id))
    toast.success(t('goals.deleted', { name: goal.name }))
    refresh()
  } catch (err) {
    toast.error(err.message)
  }
}

function refresh() {
  load()
  // A linked contribution moves real money, so the dashboard is stale either way.
  dashboard.invalidate()
}

onMounted(load)
</script>

<template>
  <div>
    <PageHeader
      :title="t('goals.title')"
      :subtitle="t('goals.subtitle')"
    >
      <template #actions>
        <BaseButton data-tour="goals-add" variant="primary" @click="openCreate">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          {{ isSavingsTab ? t('composed.newSavingsGoal') : t('composed.newProductGoal') }}
        </BaseButton>
      </template>
    </PageHeader>

    <div v-if="summary.total" class="summary">
      <div class="summary__item">
        <span class="summary__value numeric">{{ summary.total }}</span>
        <span class="summary__label">{{ t('goals.title') }}</span>
      </div>
      <div class="summary__item">
        <span class="summary__value numeric">{{ summary.done }}</span>
        <span class="summary__label">{{ t('goals.completed') }}</span>
      </div>
      <div class="summary__item" :class="{ 'is-warning': summary.behind > 0 }">
        <span class="summary__value numeric">{{ summary.behind }}</span>
        <span class="summary__label">{{ t('goals.offPace') }}</span>
      </div>
    </div>

    <BaseTabs v-model="tab" :tabs="tabs" class="tabs" data-tour="goals-tabs" />

    <CardGridSkeleton v-if="loading && !visible.length" />

    <BaseEmptyState
      v-else-if="!visible.length"
      :icon="isSavingsTab ? 'target' : 'bag'"
      :title="isSavingsTab ? t('goals.noSavingsGoals') : t('goals.noProductGoals')"
      :message="isSavingsTab ? t('goals.savingsEmptyMessage') : t('goals.productEmptyMessage')"
    >
      <template #action>
        <BaseButton variant="primary" @click="openCreate">
          {{ isSavingsTab ? t('composed.createSavingsGoal') : t('composed.createProductGoal') }}
        </BaseButton>
      </template>
    </BaseEmptyState>

    <motion.div
      v-else
      class="grid"
      v-bind="fadeUp()"
    >
      <GoalCard
        v-for="goal in visible"
        :key="goal.id"
        :goal="goal"
        :kind="tab"
        @contribute="openContribute"
        @edit="openEdit"
        @delete="remove"
      />
    </motion.div>

    <GoalFormModal v-model="formOpen" :goal="editing" :kind="tab" @saved="refresh" />
    <ContributeModal v-model="contributeOpen" :goal="active" :kind="tab" @saved="refresh" />
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/goals/GoalsView.scss"></style>
