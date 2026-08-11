<script setup>
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useMoney } from '@/composables/useMoney'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseProgress from '@/components/base/BaseProgress.vue'

const props = defineProps({
  plan: { type: Object, required: true }
})

defineEmits(['pay'])

const { format } = useMoney()

const paidCount = computed(() => props.plan.monthsCount - props.plan.remainingMonths)
const paidAmount = computed(() => props.plan.totalPrice - props.plan.remainingBalance)
</script>

<template>
  <article class="plan">
    <RouterLink :to="`/installments/${plan.id}`" class="plan__main">
      <header class="plan__head">
        <div class="plan__identity">
          <BaseIcon name="layers" :size="16" />
          <h3 class="plan__name">{{ plan.productName }}</h3>
        </div>

        <BaseBadge v-if="plan.isCompleted" variant="positive" size="sm">Paid off</BaseBadge>
        <BaseBadge v-else size="sm">{{ paidCount }}/{{ plan.monthsCount }}</BaseBadge>
      </header>

      <div class="plan__figures">
        <div>
          <span class="plan__label">Remaining</span>
          <p class="plan__remaining numeric">
            {{ format(plan.remainingBalance, { currency: plan.currency }) }}
          </p>
        </div>

        <div class="plan__monthly">
          <span class="plan__label">Monthly</span>
          <p class="numeric">{{ format(plan.monthlyPayment, { currency: plan.currency }) }}</p>
        </div>
      </div>

      <BaseProgress
        :value="paidAmount"
        :max="plan.totalPrice"
        :variant="plan.isCompleted ? 'positive' : 'accent'"
        size="sm"
      >
        <template #label>
          <span class="plan__paid numeric">
            {{ format(paidAmount, { currency: plan.currency }) }} of
            {{ format(plan.totalPrice, { currency: plan.currency }) }}
          </span>
        </template>
      </BaseProgress>

      <p class="plan__end">
        {{ plan.isCompleted ? 'Completed' : `Ends ${plan.endDate}` }}
      </p>
    </RouterLink>

    <footer class="plan__actions">
      <BaseButton
        size="sm"
        variant="secondary"
        :disabled="plan.isCompleted"
        @click="$emit('pay', plan)"
      >
        <template #icon><BaseIcon name="receipt" :size="14" /></template>
        Pay next
      </BaseButton>
    </footer>
  </article>
</template>

<style scoped lang="scss">
.plan {
  display: flex;
  flex-direction: column;
  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
  overflow: hidden;
}

.plan__main {
  display: flex;
  flex-direction: column;
  flex: 1;
  gap: var(--sp-3);
  padding: var(--sp-4) var(--sp-5);
  color: inherit;
  text-decoration: none;

  &:hover { background: var(--canvas-alt); text-decoration: none; }
}

.plan__head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--sp-3);
}

.plan__identity {
  display: flex;
  align-items: center;
  gap: var(--sp-3);
  min-width: 0;
  color: var(--text-muted);
}

.plan__name {
  font-size: var(--fs-md);
  font-weight: var(--fw-semibold);
  color: var(--text);
  @include truncate;
}

.plan__figures {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--sp-4);
}

.plan__label {
  font-size: var(--fs-xs);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-muted);
}

.plan__remaining {
  font-size: var(--fs-xl);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.02em;
}

.plan__monthly { text-align: right; font-weight: var(--fw-medium); }
.plan__paid { font-size: var(--fs-xs); color: var(--text-muted); }
.plan__end { font-size: var(--fs-xs); color: var(--text-subtle); }

.plan__actions {
  display: flex;
  gap: var(--sp-2);
  padding: var(--sp-3) var(--sp-4);
  border-top: var(--border-subtle);
  background: var(--canvas-alt);
}
</style>
