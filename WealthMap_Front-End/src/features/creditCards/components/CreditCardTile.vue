<script setup>
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useMoney } from '@/composables/useMoney'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseProgress from '@/components/base/BaseProgress.vue'

const props = defineProps({
  card: { type: Object, required: true }
})

defineEmits(['pay', 'edit', 'limit'])

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
            <p class="card__bank">{{ card.bankName }}</p>
          </div>
        </div>

        <span class="card__due">Due day {{ card.paymentDueDay }}</span>
      </header>

      <div class="card__figures">
        <div>
          <span class="card__figure-label">Available</span>
          <p class="card__available numeric">
            {{ format(card.availableCredit, { currency: card.currency }) }}
          </p>
        </div>

        <div class="card__owed">
          <span class="card__figure-label">Owed</span>
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
            Limit {{ format(card.creditLimit, { currency: card.currency }) }}
          </span>
        </template>
      </BaseProgress>
    </RouterLink>

    <footer class="card__actions">
      <BaseButton
        size="sm"
        variant="secondary"
        :disabled="card.usedCredit <= 0"
        :title="card.usedCredit <= 0 ? 'Nothing owed on this card' : undefined"
        @click="$emit('pay', card)"
      >
        <template #icon><BaseIcon name="receipt" :size="14" /></template>
        Pay
      </BaseButton>

      <BaseButton size="sm" variant="ghost" @click="$emit('limit', card)">
        Limit
      </BaseButton>

      <div class="card__spacer" />

      <BaseButton size="sm" variant="ghost" title="Edit" @click="$emit('edit', card)">
        <template #icon><BaseIcon name="pencil" :size="14" /></template>
        <span class="sr-only">Edit</span>
      </BaseButton>
    </footer>
  </article>
</template>

<style scoped lang="scss">
.card {
  display: flex;
  flex-direction: column;
  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
  overflow: hidden;
}

.card__main {
  display: flex;
  flex-direction: column;
  flex: 1;
  gap: var(--sp-4);
  padding: var(--sp-4) var(--sp-5);
  color: inherit;
  text-decoration: none;

  &:hover { background: var(--canvas-alt); text-decoration: none; }
}

.card__head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--sp-3);
}

.card__identity {
  display: flex;
  align-items: center;
  gap: var(--sp-3);
  min-width: 0;
  color: var(--text-muted);
}

.card__name {
  font-size: var(--fs-md);
  font-weight: var(--fw-semibold);
  color: var(--text);
  @include truncate;
}

.card__bank { font-size: var(--fs-xs); color: var(--text-muted); }

.card__due {
  flex: none;
  font-size: var(--fs-xs);
  color: var(--text-muted);
  padding: 2px var(--sp-2);
  background: var(--canvas-alt);
  border-radius: var(--radius-sm);
}

.card__figures {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--sp-4);
}

.card__figure-label {
  font-size: var(--fs-xs);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-muted);
}

.card__available {
  font-size: var(--fs-xl);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.02em;
  color: var(--positive);
}

.card__owed {
  text-align: right;
  font-size: var(--fs-base);
  font-weight: var(--fw-medium);
}

.card__limit { font-size: var(--fs-xs); color: var(--text-muted); }

.card__actions {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: var(--sp-2);
  padding: var(--sp-3) var(--sp-4);
  border-top: var(--border-subtle);
  background: var(--canvas-alt);
}

.card__spacer { flex: 1; }
</style>
