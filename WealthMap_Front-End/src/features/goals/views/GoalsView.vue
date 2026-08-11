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
  { value: 'savings', label: 'Savings', count: savings.value.length },
  { value: 'product', label: 'Products', count: products.value.length }
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
    title: `Delete ${goal.name}?`,
    message: 'The goal and its progress are removed. Money already in a linked account stays put.',
    confirmLabel: 'Delete',
    variant: 'danger'
  })

  if (!confirmed) return

  try {
    await (isSavingsTab.value ? savingsGoalsApi.remove(goal.id) : productGoalsApi.remove(goal.id))
    toast.success(`${goal.name} deleted.`)
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
      title="Goals"
      subtitle="What you are saving toward, and what it takes each month to get there."
    >
      <template #actions>
        <BaseButton variant="primary" @click="openCreate">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          New {{ isSavingsTab ? 'savings goal' : 'product goal' }}
        </BaseButton>
      </template>
    </PageHeader>

    <div v-if="summary.total" class="summary">
      <div class="summary__item">
        <span class="summary__value numeric">{{ summary.total }}</span>
        <span class="summary__label">Goals</span>
      </div>
      <div class="summary__item">
        <span class="summary__value numeric">{{ summary.done }}</span>
        <span class="summary__label">Completed</span>
      </div>
      <div class="summary__item" :class="{ 'is-warning': summary.behind > 0 }">
        <span class="summary__value numeric">{{ summary.behind }}</span>
        <span class="summary__label">Off pace</span>
      </div>
    </div>

    <BaseTabs v-model="tab" :tabs="tabs" class="tabs" />

    <CardGridSkeleton v-if="loading && !visible.length" />

    <BaseEmptyState
      v-else-if="!visible.length"
      :icon="isSavingsTab ? 'target' : 'bag'"
      :title="isSavingsTab ? 'No savings goals yet' : 'No product goals yet'"
      :message="isSavingsTab
        ? 'Set a target and a deadline, and WealthMap works out what to put aside each month.'
        : 'Saving for something specific? Track it here — a deadline is optional.'"
    >
      <template #action>
        <BaseButton variant="primary" @click="openCreate">
          Create {{ isSavingsTab ? 'a savings goal' : 'a product goal' }}
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

<style scoped lang="scss">
.summary {
  display: flex;
  flex-wrap: wrap;
  gap: var(--sp-3);
  margin-bottom: var(--sp-5);
}

.summary__item {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 96px;

  padding: var(--sp-3) var(--sp-4);
  background: var(--canvas-alt);
  border: var(--border-subtle);
  border-radius: var(--radius);

  &.is-warning {
    background: var(--warning-soft);
    border-color: var(--warning);
  }
}

.summary__value { font-size: var(--fs-lg); font-weight: var(--fw-semibold); }

.summary__label {
  font-size: var(--fs-xs);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-muted);
}

.tabs { margin-bottom: var(--sp-4); }

.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
  gap: var(--sp-4);
}


@media (max-width: 640px) {
  .grid { grid-template-columns: 1fr; }
}
</style>
