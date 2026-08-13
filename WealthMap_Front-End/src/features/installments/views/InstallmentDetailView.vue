<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
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
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

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
      {{ t('installments.allPlans') }}
    </RouterLink>

    <div v-if="loading && !plan" class="state"><BaseSpinner :size="22" /></div>

    <BaseEmptyState
      v-else-if="error"
      icon="alert"
      :title="t('installments.notFound')"
      :message="t('common.notFoundHint')"
    >
      <template #action>
        <BaseButton variant="secondary" @click="$router.push('/installments')">{{ t('installments.backToPlans') }}</BaseButton>
      </template>
    </BaseEmptyState>

    <motion.div v-else-if="plan" v-bind="fadeUp()">
      <PageHeader
        :title="plan.productName"
        :subtitle="t('installments.planSubtitle', { count: plan.monthsCount, date: plan.purchasedAt })"
      >
        <template #actions>
          <BaseButton variant="primary" :disabled="plan.isCompleted" @click="payOpen = true">
            <template #icon><BaseIcon name="receipt" :size="15" /></template>
            {{ t('installments.payNextTitle') }}
          </BaseButton>
        </template>
      </PageHeader>

      <div class="summary">
        <div class="summary__figures">
          <div>
            <span class="summary__label">{{ t('installments.remaining') }}</span>
            <p class="summary__value numeric">
              {{ format(plan.remainingBalance, { currency: plan.currency }) }}
            </p>
          </div>
          <div>
            <span class="summary__label">{{ t('common.monthly') }}</span>
            <p class="summary__value numeric">
              {{ format(plan.monthlyPayment, { currency: plan.currency }) }}
            </p>
          </div>
          <div>
            <span class="summary__label">{{ t('installments.totalPrice') }}</span>
            <p class="summary__value numeric">
              {{ format(plan.totalPrice, { currency: plan.currency }) }}
            </p>
          </div>
        </div>

        <BaseProgress
          :value="paidAmount"
          :max="plan.totalPrice"
          :variant="plan.isCompleted ? 'positive' : 'accent'"
          :label="plan.isCompleted ? t('installments.fullyPaid') : t('installments.monthsLeft', { remaining: plan.remainingMonths, total: plan.monthsCount, date: plan.endDate })"
        />
      </div>

      <BaseCard :title="t('installments.schedule')" :subtitle="t('installments.scheduleSubtitle')" :padded="false">
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
              <span class="schedule__due">{{ t('composed.dueOn', { date: item.dueDate }) }}</span>
            </div>

            <BaseBadge v-if="item.isPaid" variant="positive" size="sm">{{ t('common.paid') }}</BaseBadge>
            <BaseBadge v-else-if="item.number === nextNumber" variant="warning" size="sm">{{ t('common.next') }}</BaseBadge>
            <BaseBadge v-else size="sm">{{ t('installments.scheduled') }}</BaseBadge>
          </li>
        </ol>
      </BaseCard>

      <PayInstallmentModal v-model="payOpen" :plan="plan" @saved="refresh" />
    </motion.div>
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/installments/InstallmentDetailView.scss"></style>
