<script setup>
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useMoney } from '@/composables/useMoney'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseProgress from '@/components/base/BaseProgress.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'

const { t, tc } = useI18n()

const props = defineProps({
  goals: { type: Object, default: null }
})

const { format } = useMoney()

const percent = computed(() => {
  if (!props.goals?.totalTargeted) return 0
  return (props.goals.totalSaved / props.goals.totalTargeted) * 100
})
</script>

<template>
  <BaseCard :title="t('dashboard.goalsTitle')">
    <BaseEmptyState
      v-if="!goals || goals.total === 0"
      icon="target"
      :title="t('dashboard.noGoalsTitle')"
      :message="t('dashboard.noGoalsMessage')"
      compact
    >
      <template #action>
        <BaseButton variant="secondary" size="sm" @click="$router.push('/goals')">
          {{ t('dashboard.createGoal') }}
        </BaseButton>
      </template>
    </BaseEmptyState>

    <template v-else>
      <BaseProgress
        :value="goals.totalSaved"
        :max="goals.totalTargeted"
        variant="positive"
      >
        <template #label>
          <span class="goals__saved numeric">{{ format(goals.totalSaved) }}</span>
          <span class="goals__of">of {{ format(goals.totalTargeted) }}</span>
        </template>
      </BaseProgress>

      <ul class="goals__stats">
        <li class="goals__stat">
          <span class="goals__stat-value numeric">{{ goals.total }}</span>
          <span class="goals__stat-label">{{ t('common.total') }}</span>
        </li>
        <li class="goals__stat">
          <span class="goals__stat-value numeric">{{ goals.completed }}</span>
          <span class="goals__stat-label">{{ t('common.completed') }}</span>
        </li>
        <li class="goals__stat">
          <span class="goals__stat-value numeric">{{ goals.behindSchedule }}</span>
          <span class="goals__stat-label">{{ t('dashboard.behind') }}</span>
        </li>
      </ul>

      <BaseBadge v-if="goals.behindSchedule > 0" variant="warning" size="sm">
        {{ tc('composed.goalsOffPace', goals.behindSchedule) }}
      </BaseBadge>
    </template>

    <template #footer>
      <RouterLink to="/goals" class="goals__link">{{ t('dashboard.viewAllGoals') }}</RouterLink>
    </template>
  </BaseCard>
</template>

<style scoped lang="scss" src="@/assets/styles/features/dashboard/GoalsSummaryCard.scss"></style>
