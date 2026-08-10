<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { installmentsApi } from '@/api/installments.api'
import { useAsync } from '@/composables/useAsync'
import { useMoney } from '@/composables/useMoney'
import { useDashboardStore } from '@/stores/dashboard.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseProgress from '@/components/base/BaseProgress.vue'
import BaseSpinner from '@/components/base/BaseSpinner.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'

import PayInstallmentModal from '../components/PayInstallmentModal.vue'

const route = useRoute()
const { format } = useMoney()
const dashboard = useDashboardStore()

const planId = route.params.id

const { data: plan, loading, error, run: loadPlan } = useAsync(() => installmentsApi.get(planId))

const payOpen = ref(false)

const paidAmount = computed(() =>
  plan.value ? plan.value.totalPrice - plan.value.remainingBalance : 0
)

const schedule = computed(() =>
  [...(plan.value?.payments ?? [])].sort((a, b) => a.number - b.number)
)

const nextNumber = computed(() => schedule.value.find((p) => !p.isPaid)?.number ?? null)

function refresh() {
  loadPlan()
  dashboard.invalidate()
}

onMounted(loadPlan)
</script>

<template>
  <div>
    <RouterLink to="/installments" class="back">
      <BaseIcon name="chevron-left" :size="15" />
      All plans
    </RouterLink>

    <div v-if="loading && !plan" class="state"><BaseSpinner :size="22" /></div>

    <BaseEmptyState
      v-else-if="error"
      icon="alert"
      title="Plan not found"
      message="It may have been removed, or it is not yours."
    >
      <template #action>
        <BaseButton variant="secondary" @click="$router.push('/installments')">Back to plans</BaseButton>
      </template>
    </BaseEmptyState>

    <template v-else-if="plan">
      <PageHeader
        :title="plan.productName"
        :subtitle="`${plan.monthsCount} interest-free payments · purchased ${plan.purchasedAt}`"
      >
        <template #actions>
          <BaseButton variant="primary" :disabled="plan.isCompleted" @click="payOpen = true">
            <template #icon><BaseIcon name="receipt" :size="15" /></template>
            Pay next installment
          </BaseButton>
        </template>
      </PageHeader>

      <div class="summary">
        <div class="summary__figures">
          <div>
            <span class="summary__label">Remaining</span>
            <p class="summary__value numeric">
              {{ format(plan.remainingBalance, { currency: plan.currency }) }}
            </p>
          </div>
          <div>
            <span class="summary__label">Monthly</span>
            <p class="summary__value numeric">
              {{ format(plan.monthlyPayment, { currency: plan.currency }) }}
            </p>
          </div>
          <div>
            <span class="summary__label">Total price</span>
            <p class="summary__value numeric">
              {{ format(plan.totalPrice, { currency: plan.currency }) }}
            </p>
          </div>
        </div>

        <BaseProgress
          :value="paidAmount"
          :max="plan.totalPrice"
          :variant="plan.isCompleted ? 'positive' : 'accent'"
          :label="plan.isCompleted ? 'Fully paid' : `${plan.remainingMonths} of ${plan.monthsCount} left · ends ${plan.endDate}`"
        />
      </div>

      <BaseCard title="Schedule" subtitle="Generated when the plan was created" :padded="false">
        <ol class="schedule">
          <li
            v-for="item in schedule"
            :key="item.id"
            class="schedule__item"
            :class="{ 'is-paid': item.isPaid, 'is-next': item.number === nextNumber }"
          >
            <span class="schedule__number numeric">{{ item.number }}</span>

            <div class="schedule__body">
              <span class="schedule__amount numeric">
                {{ format(item.amount, { currency: item.currency }) }}
              </span>
              <span class="schedule__due">Due {{ item.dueDate }}</span>
            </div>

            <BaseBadge v-if="item.isPaid" variant="positive" size="sm">Paid</BaseBadge>
            <BaseBadge v-else-if="item.number === nextNumber" variant="warning" size="sm">Next</BaseBadge>
            <BaseBadge v-else size="sm">Scheduled</BaseBadge>
          </li>
        </ol>
      </BaseCard>

      <PayInstallmentModal v-model="payOpen" :plan="plan" @saved="refresh" />
    </template>
  </div>
</template>

<style scoped lang="scss">
.back {
  display: inline-flex;
  align-items: center;
  gap: var(--sp-1);
  margin-bottom: var(--sp-4);
  font-size: var(--fs-sm);
  color: var(--text-muted);

  &:hover { color: var(--text); text-decoration: none; }
}

.summary {
  display: flex;
  flex-direction: column;
  gap: var(--sp-5);

  padding: var(--sp-5);
  margin-bottom: var(--sp-5);

  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
}

.summary__figures {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--sp-4);
}

.summary__label {
  font-size: var(--fs-xs);
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--text-muted);
}

.summary__value {
  font-size: var(--fs-xl);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.02em;
}

.schedule { display: flex; flex-direction: column; }

.schedule__item {
  display: flex;
  align-items: center;
  gap: var(--sp-3);

  padding: var(--sp-3) var(--sp-5);
  border-bottom: var(--border-subtle);

  &:last-child { border-bottom: none; }

  &.is-paid { background: var(--canvas-alt); }
  &.is-next { border-left: 3px solid var(--warning); }
}

.schedule__number {
  display: grid;
  place-items: center;
  width: 26px;
  height: 26px;
  flex: none;

  border: var(--border-subtle);
  border-radius: 50%;
  font-size: var(--fs-xs);
  color: var(--text-muted);
}

.is-paid .schedule__number {
  background: var(--positive-soft);
  border-color: var(--positive);
  color: var(--positive);
}

.schedule__body { flex: 1; display: flex; flex-direction: column; }
.schedule__amount { font-weight: var(--fw-semibold); }
.schedule__due { font-size: var(--fs-xs); color: var(--text-muted); }

.state { display: grid; place-items: center; padding: var(--sp-12); color: var(--text-muted); }

@media (max-width: 640px) {
  .summary__figures { grid-template-columns: 1fr; gap: var(--sp-3); }
  .summary__value { font-size: var(--fs-lg); }
  .schedule__item { padding: var(--sp-3) var(--sp-4); }
}
</style>
