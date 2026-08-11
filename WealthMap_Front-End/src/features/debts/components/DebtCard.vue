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
            {{ format(paid, { currency: debt.currency }) }} of
            {{ format(debt.originalAmount, { currency: debt.currency }) }} repaid
          </span>
        </template>
      </BaseProgress>

      <p class="debt__due">
        <template v-if="debt.nextDueDate">Next due {{ debt.nextDueDate }}</template>
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

<style scoped lang="scss">
.debt {
  display: flex;
  flex-direction: column;
  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
  overflow: hidden;
}

.debt--defaulted { border-left: 3px solid var(--negative); }

.debt__main {
  display: flex;
  flex-direction: column;
  flex: 1;
  gap: var(--sp-3);
  padding: var(--sp-4) var(--sp-5);
  color: inherit;
  text-decoration: none;

  &:hover { background: var(--canvas-alt); text-decoration: none; }
}

.debt__head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--sp-3);
}

.debt__identity {
  display: flex;
  align-items: center;
  gap: var(--sp-3);
  min-width: 0;
  color: var(--text-muted);
}

.debt__name {
  font-size: var(--fs-md);
  font-weight: var(--fw-semibold);
  color: var(--text);
  @include truncate;
}

.debt__figures {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--sp-4);
}

.debt__label {
  font-size: var(--fs-xs);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-muted);
}

.debt__remaining {
  font-size: var(--fs-xl);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.02em;
}

.debt__monthly { text-align: right; font-weight: var(--fw-medium); }
.debt__paid { font-size: var(--fs-xs); color: var(--text-muted); }
.debt__due { font-size: var(--fs-xs); color: var(--text-subtle); }

.debt__actions {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: var(--sp-2);
  padding: var(--sp-3) var(--sp-4);
  border-top: var(--border-subtle);
  background: var(--canvas-alt);
}

.debt__spacer { flex: 1; }
</style>
