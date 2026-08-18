<script setup>
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useMoney } from '@/composables/useMoney'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseProgress from '@/components/base/BaseProgress.vue'
import { useI18n } from '@/composables/useI18n'
import { useDateTime } from '@/composables/useDateTime'

const { t } = useI18n()
const { formatDate, relativeDay } = useDateTime()

const props = defineProps({
  card: { type: Object, required: true }
})

defineEmits(['pay', 'edit', 'limit', 'delete'])

const { format } = useMoney()

const utilisation = computed(() => {
  if (!props.card.creditLimit) return 0
  return (props.card.usedCredit / props.card.creditLimit) * 100
})

/** High utilisation is the signal worth colouring; the thresholds are conventional. */
const variant = computed(() => {
  if (utilisation.value >= 80) return 'negative'
  if (utilisation.value >= 50) return 'warning'
  return 'accent'
})
</script>

<template>
  <article class="card">
    <RouterLink :to="`/credit-cards/${card.id}`" class="card__main">
      <header class="card__head">
        <div class="card__identity">
          <BaseIcon name="card" :size="16" />
          <div>
            <h3 class="card__name">{{ card.cardName }}</h3>
            <p class="card__bank">
              {{ card.bankName }}
              <!-- Only when set: a placeholder would imply data that is simply absent. -->
              <span v-if="card.lastFour" class="numeric card__last-four">••••{{ card.lastFour }}</span>
            </p>
          </div>
        </div>

        <span :class="['card__due', { 'card__due--soon': card.daysUntilDue <= 7 }]">
          {{ relativeDay(card.daysUntilDue) }}
        </span>
      </header>

      <div class="card__figures">
        <div>
          <span class="card__figure-label">{{ t('cards.available') }}</span>
          <p class="card__available numeric">
            {{ format(card.availableCredit, { currency: card.currency }) }}
          </p>
        </div>

        <div class="card__owed">
          <span class="card__figure-label">{{ t('cards.owed') }}</span>
          <p class="numeric">{{ format(card.usedCredit, { currency: card.currency }) }}</p>
        </div>
      </div>

      <BaseProgress
        :value="card.usedCredit"
        :max="card.creditLimit"
        :variant="variant"
        size="sm"
      >
        <template #label>
          <span class="card__limit">
            {{ t('cards.limit') }} {{ format(card.creditLimit, { currency: card.currency }) }}
          </span>
        </template>
      </BaseProgress>

      <!-- The split, not just the total: owing 100 with 50 due on the 15th and 50
           not billed for another month is a different obligation from owing 100
           all at once. The dates are shown beside each figure because each is
           only meaningful with its deadline. -->
      <dl class="card__cycle">
        <div class="card__cycle-item">
          <dt>
            {{ t('cards.dueThisStatement') }}
            <span class="card__cycle-when">{{ formatDate(card.nextDueDate) }}</span>
          </dt>
          <dd :class="['numeric', { 'is-negative': card.statementBalance > 0 }]">
            {{ format(card.statementBalance, { currency: card.currency }) }}
          </dd>
        </div>

        <div class="card__cycle-item">
          <dt>
            {{ t('cards.nextStatement') }}
            <span class="card__cycle-when">{{ t('composed.closesOn', { date: formatDate(card.nextCutoffDate) }) }}</span>
          </dt>
          <dd class="numeric">
            {{ format(card.currentCycleCharges, { currency: card.currency }) }}
          </dd>
        </div>
      </dl>

      <!-- Only when a plan is running: otherwise the three figures would not add
           up to the total and the reader would be left hunting for the gap. -->
      <p v-if="card.futureInstallments > 0" class="card__future">
        {{ t('composed.plusFutureInstallments', {
          amount: format(card.futureInstallments, { currency: card.currency })
        }) }}
      </p>
    </RouterLink>

    <footer class="card__actions">
      <BaseButton
        size="sm"
        variant="secondary"
        :disabled="card.usedCredit <= 0"
        :title="card.usedCredit <= 0 ? t('cards.nothingOwed') : undefined"
        data-tour="cards-pay"
        @click="$emit('pay', card)"
      >
        <template #icon><BaseIcon name="receipt" :size="14" /></template>
        {{ t('cards.pay') }}
      </BaseButton>

      <BaseButton size="sm" variant="ghost" @click="$emit('limit', card)">
        {{ t('cards.limit') }}
      </BaseButton>

      <div class="card__spacer" />

      <BaseButton
        size="sm"
        variant="ghost"
        :title="t('common.edit')"
        :aria-label="t('cards.editCard')"
        @click="$emit('edit', card)"
      >
        <template #icon><BaseIcon name="pencil" :size="14" /></template>
      </BaseButton>

      <BaseButton
        class="card__delete"
        size="sm"
        variant="ghost"
        :title="t('common.delete')"
        :aria-label="t('cards.deleteAria')"
        @click="$emit('delete', card)"
      >
        <template #icon><BaseIcon name="trash" :size="14" /></template>
      </BaseButton>
    </footer>
  </article>
</template>

<style scoped lang="scss" src="@/assets/styles/features/creditCards/CreditCardTile.scss"></style>
