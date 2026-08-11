<script setup>
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useMoney } from '@/composables/useMoney'
import { DEBT_STATUS_VARIANT } from '@/api/debts.api'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseProgress from '@/components/base/BaseProgress.vue'
import { useI18n } from '@/composables/useI18n'
import { useServerText } from '@/composables/useServerText'

const { t } = useI18n()
const { label: serverLabel } = useServerText()

const props = defineProps({
  debt: { type: Object, required: true }
})

defineEmits(['pay', 'edit', 'default', 'delete'])

const { format } = useMoney()

const paid = computed(() => props.debt.originalAmount - props.debt.remainingAmount)
const isPaidOff = computed(() => props.debt.status === 'PaidOff')
</script>

<template>
  <article class="debt" :class="{ 'debt--defaulted': debt.status === 'Defaulted' }">
    <RouterLink :to="`/debts/${debt.id}`" class="debt__main">
      <header class="debt__head">
        <div class="debt__identity">
          <BaseIcon name="debt" :size="16" />
          <h3 class="debt__name">{{ debt.name }}</h3>
        </div>

        <BaseBadge :variant="DEBT_STATUS_VARIANT[debt.status] ?? 'neutral'" size="sm">
          {{ serverLabel('debtStatus', debt.status) }}
        </BaseBadge>
      </header>

      <div class="debt__figures">
        <div>
          <span class="debt__label">{{ t('debts.remaining') }}</span>
          <p class="debt__remaining numeric">
            {{ format(debt.remainingAmount, { currency: debt.currency }) }}
          </p>
        </div>

        <div class="debt__monthly">
          <span class="debt__label">{{ t('debts.monthly') }}</span>
          <p class="numeric">{{ format(debt.monthlyPayment, { currency: debt.currency }) }}</p>
        </div>
      </div>

      <BaseProgress
        :value="paid"
        :max="debt.originalAmount"
        :variant="isPaidOff ? 'positive' : 'accent'"
        size="sm"
      >
        <template #label>
          <span class="debt__paid numeric">
            {{ t('debts.repaidOf', {
              paid: format(paid, { currency: debt.currency }),
              total: format(debt.originalAmount, { currency: debt.currency })
            }) }}
          </span>
        </template>
      </BaseProgress>

      <p class="debt__due">
        <template v-if="debt.nextDueDate">{{ t('composed.nextDue', { date: debt.nextDueDate }) }}</template>
        <template v-else>{{ t('debts.nothingFurtherDue') }}</template>
      </p>
    </RouterLink>

    <footer class="debt__actions">
      <BaseButton size="sm" variant="secondary" :disabled="isPaidOff" @click="$emit('pay', debt)">
        <template #icon><BaseIcon name="receipt" :size="14" /></template>
        Pay
      </BaseButton>

      <BaseButton
        v-if="debt.status === 'Active'"
        size="sm"
        variant="ghost"
        :title="t('debts.markDefaulted')"
        @click="$emit('default', debt)"
      >
        Default
      </BaseButton>

      <div class="debt__spacer" />

      <BaseButton
        size="sm"
        variant="ghost"
        :title="t('common.edit')"
        :aria-label="t('debts.editDebt')"
        @click="$emit('edit', debt)"
      >
        <template #icon><BaseIcon name="pencil" :size="14" /></template>
      </BaseButton>

      <BaseButton
        size="sm"
        variant="ghost"
        :title="t('common.delete')"
        :aria-label="t('debts.deleteAria')"
        @click="$emit('delete', debt)"
      >
        <template #icon><BaseIcon name="trash" :size="14" /></template>
      </BaseButton>
    </footer>
  </article>
</template>

<style scoped lang="scss" src="./DebtCard.scss"></style>
