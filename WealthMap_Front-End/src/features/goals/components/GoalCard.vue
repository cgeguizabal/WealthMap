<script setup>
import { computed } from 'vue'
import { useMoney } from '@/composables/useMoney'
import { GOAL_STATUS_VARIANT, GOAL_STATUS_LABEL } from '@/api/goals.api'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseProgress from '@/components/base/BaseProgress.vue'

const props = defineProps({
  goal: { type: Object, required: true },
  kind: { type: String, required: true, validator: (v) => ['savings', 'product'].includes(v) }
})

defineEmits(['contribute', 'edit', 'delete'])

const { format } = useMoney()

const isComplete = computed(() => props.goal.status === 'Completed')
const isLinked = computed(() => Boolean(props.goal.linkedAccountId))

const progressVariant = computed(() => {
  if (isComplete.value) return 'positive'
  if (props.goal.status === 'DeadlinePassed') return 'negative'
  if (props.goal.status === 'BehindSchedule') return 'warning'
  return 'accent'
})

/** Null means no deadline, or one that has passed — neither yields a monthly figure. */
const hasRequired = computed(() =>
  props.goal.requiredMonthlyContribution !== null &&
  props.goal.requiredMonthlyContribution !== undefined
)
</script>

<template>
  <article class="goal">
    <header class="goal__head">
      <div class="goal__identity">
        <BaseIcon :name="kind === 'savings' ? 'target' : 'bag'" :size="16" />
        <h3 class="goal__name">{{ goal.name }}</h3>
      </div>

      <BaseBadge :variant="GOAL_STATUS_VARIANT[goal.status] ?? 'neutral'" size="sm">
        {{ GOAL_STATUS_LABEL[goal.status] ?? goal.status }}
      </BaseBadge>
    </header>

    <div class="goal__amounts">
      <p class="goal__current numeric">{{ format(goal.currentAmount, { currency: goal.currency }) }}</p>
      <p class="goal__target numeric">of {{ format(goal.targetAmount, { currency: goal.currency }) }}</p>
    </div>

    <BaseProgress
      :value="goal.currentAmount"
      :max="goal.targetAmount"
      :variant="progressVariant"
      :label="`${goal.progressPercentage.toFixed(1)}% funded`"
    />

    <dl class="goal__meta">
      <div v-if="goal.deadline">
        <dt>Deadline</dt>
        <dd>{{ goal.deadline }}</dd>
      </div>
      <div v-else>
        <dt>Deadline</dt>
        <dd class="is-muted">None set</dd>
      </div>

      <div v-if="goal.monthsRemaining !== null && goal.monthsRemaining !== undefined">
        <dt>Months left</dt>
        <dd class="numeric">{{ goal.monthsRemaining }}</dd>
      </div>

      <!-- Computed server-side from what is left and how many months remain -->
      <div v-if="hasRequired">
        <dt>Needed monthly</dt>
        <dd class="numeric is-strong">
          {{ format(goal.requiredMonthlyContribution, { currency: goal.currency }) }}
        </dd>
      </div>
    </dl>

    <p v-if="kind === 'savings'" class="goal__mode">
      <BaseIcon :name="isLinked ? 'wallet' : 'info'" :size="13" />
      {{ isLinked
        ? 'Linked — contributing moves real money into the savings account'
        : 'Tracked only — contributing does not move money' }}
    </p>

    <footer class="goal__actions">
      <BaseButton
        size="sm"
        variant="secondary"
        :disabled="isComplete"
        :title="isComplete ? 'Target already reached' : undefined"
        @click="$emit('contribute', goal)"
      >
        <template #icon><BaseIcon name="plus" :size="14" /></template>
        Contribute
      </BaseButton>

      <div class="goal__spacer" />

      <BaseButton size="sm" variant="ghost" title="Edit" @click="$emit('edit', goal)">
        <template #icon><BaseIcon name="pencil" :size="14" /></template>
        <span class="sr-only">Edit</span>
      </BaseButton>

      <BaseButton size="sm" variant="ghost" title="Delete" @click="$emit('delete', goal)">
        <template #icon><BaseIcon name="trash" :size="14" /></template>
        <span class="sr-only">Delete</span>
      </BaseButton>
    </footer>
  </article>
</template>

<style scoped lang="scss">
.goal {
  display: flex;
  flex-direction: column;
  gap: var(--sp-3);

  padding: var(--sp-4) var(--sp-5);
  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
}

.goal__head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--sp-3);
}

.goal__identity {
  display: flex;
  align-items: center;
  gap: var(--sp-3);
  min-width: 0;
  color: var(--text-muted);
}

.goal__name {
  font-size: var(--fs-md);
  font-weight: var(--fw-semibold);
  color: var(--text);
  @include truncate;
}

.goal__amounts { display: flex; align-items: baseline; gap: var(--sp-2); }

.goal__current {
  font-size: var(--fs-xl);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.02em;
}

.goal__target { font-size: var(--fs-sm); color: var(--text-muted); }

.goal__meta {
  display: flex;
  flex-wrap: wrap;
  gap: var(--sp-5);
  padding-top: var(--sp-3);
  border-top: var(--border-subtle);

  dt {
    font-size: var(--fs-xs);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: var(--text-muted);
  }

  dd { font-size: var(--fs-sm); font-weight: var(--fw-medium); }
  .is-muted { color: var(--text-subtle); font-weight: var(--fw-normal); }
  .is-strong { font-size: var(--fs-base); font-weight: var(--fw-semibold); }
}

.goal__mode {
  display: flex;
  align-items: center;
  gap: var(--sp-2);
  font-size: var(--fs-xs);
  color: var(--text-muted);
}

.goal__actions {
  display: flex;
  align-items: center;
  gap: var(--sp-2);
  margin-top: var(--sp-1);
  padding-top: var(--sp-3);
  border-top: var(--border-subtle);
}

.goal__spacer { flex: 1; }
</style>
