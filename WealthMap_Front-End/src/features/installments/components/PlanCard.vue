<script setup>
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useMoney } from '@/composables/useMoney'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseProgress from '@/components/base/BaseProgress.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

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

        <BaseBadge v-if="plan.isCompleted" variant="positive" size="sm">{{ t('installments.paidOff') }}</BaseBadge>
        <BaseBadge v-else size="sm">{{ paidCount }}/{{ plan.monthsCount }}</BaseBadge>
      </header>

      <div class="plan__figures">
        <div>
          <span class="plan__label">{{ t('installments.remaining') }}</span>
          <p class="plan__remaining numeric">
            {{ format(plan.remainingBalance, { currency: plan.currency }) }}
          </p>
        </div>

        <div class="plan__monthly">
          <span class="plan__label">{{ t('common.monthly') }}</span>
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
            {{ t('installments.paidOf', {
              paid: format(paidAmount, { currency: plan.currency }),
              total: format(plan.totalPrice, { currency: plan.currency })
            }) }}
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
        {{ t('installments.payNext') }}
      </BaseButton>
    </footer>
  </article>
</template>

<style scoped lang="scss" src="./PlanCard.scss"></style>
