<script setup>
import { useMoney } from '@/composables/useMoney'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'

defineProps({
  items: { type: Array, default: () => [] }
})

const { format } = useMoney()

const ICON_BY_KIND = { CreditCard: 'card', Debt: 'debt', Installment: 'layers' }
const LABEL_BY_KIND = { CreditCard: 'Card', Debt: 'Debt', Installment: 'Installment' }

/** Urgency is the point of this list, so it drives the colour. */
function toneFor(daysUntil) {
  if (daysUntil <= 2) return 'critical'
  if (daysUntil <= 7) return 'soon'
  return 'later'
}

function relativeDay(days) {
  if (days < 0) return 'overdue'
  if (days === 0) return 'today'
  if (days === 1) return 'tomorrow'
  return `in ${days} days`
}
</script>

<template>
  <BaseCard title="Upcoming" subtitle="Next 30 days" :padded="false">
    <BaseEmptyState
      v-if="items.length === 0"
      icon="check-circle"
      title="Nothing due"
      message="No card, debt or installment payments in the next 30 days."
      compact
    />

    <ul v-else class="due">
      <li v-for="item in items" :key="`${item.kind}-${item.entityId}-${item.dueDate}`" class="due__item">
        <span :class="['due__marker', `due__marker--${toneFor(item.daysUntil)}`]" aria-hidden="true" />

        <BaseIcon :name="ICON_BY_KIND[item.kind] ?? 'clock'" :size="16" class="due__icon" />

        <div class="due__body">
          <p class="due__name">{{ item.name }}</p>
          <p class="due__meta">
            {{ LABEL_BY_KIND[item.kind] ?? item.kind }} · {{ item.dueDate }}
          </p>
        </div>

        <div class="due__right">
          <p class="due__amount numeric">{{ format(item.amount) }}</p>
          <p :class="['due__when', `due__when--${toneFor(item.daysUntil)}`]">
            {{ relativeDay(item.daysUntil) }}
          </p>
        </div>
      </li>
    </ul>
  </BaseCard>
</template>

<style scoped lang="scss">
.due { display: flex; flex-direction: column; }

.due__item {
  position: relative;
  display: flex;
  align-items: center;
  gap: var(--sp-3);

  padding: var(--sp-3) var(--sp-5);
  border-bottom: var(--border-subtle);

  &:last-child { border-bottom: none; }
}

.due__marker {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 3px;
}

.due__marker--critical { background: var(--negative); }
.due__marker--soon { background: var(--warning); }
.due__marker--later { background: transparent; }

.due__icon { color: var(--text-muted); flex: none; }
.due__body { flex: 1; min-width: 0; }

.due__name {
  font-size: var(--fs-base);
  font-weight: var(--fw-medium);
  @include truncate;
}

.due__meta { font-size: var(--fs-xs); color: var(--text-muted); }

.due__right { text-align: right; flex: none; }
.due__amount { font-size: var(--fs-base); font-weight: var(--fw-semibold); }

.due__when { font-size: var(--fs-xs); }
.due__when--critical { color: var(--negative); font-weight: var(--fw-semibold); }
.due__when--soon { color: var(--warning); }
.due__when--later { color: var(--text-muted); }

@media (max-width: 767px) {
  .due__item { padding: var(--sp-3) var(--sp-4); }
}
</style>
