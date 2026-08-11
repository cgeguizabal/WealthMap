<script setup>
import { useMoney } from '@/composables/useMoney'
import { useDateTime } from '@/composables/useDateTime'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'

defineProps({
  spending: { type: Object, required: true },
  currency: { type: String, required: true }
})

const { format } = useMoney()
const { formatDateTime } = useDateTime()

const METHOD_LABEL = { DebitAccount: 'Debit', CreditCard: 'Credit card', Cash: 'Cash' }
</script>

<template>
  <div class="spending">
    <BaseCard title="Spending by category" :padded="false">
      <BaseEmptyState
        v-if="!spending.byCategory.length"
        icon="bag"
        title="No purchases this month"
        compact
      />

      <ul v-else class="categories">
        <li v-for="entry in spending.byCategory" :key="entry.category" class="category">
          <div class="category__head">
            <span class="category__name">{{ entry.category }}</span>
            <span class="category__total numeric">
              {{ format(entry.total, { currency }) }}
            </span>
          </div>

          <div class="category__bar" aria-hidden="true">
            <div class="category__fill" :style="{ width: `${entry.sharePercentage}%` }" />
          </div>

          <div class="category__meta">
            <span>{{ entry.count }} item{{ entry.count === 1 ? '' : 's' }}</span>
            <span class="numeric">{{ entry.sharePercentage.toFixed(1) }}%</span>
          </div>
        </li>
      </ul>

      <template #footer>
        <div class="spending__footer">
          <span>Total spent</span>
          <span class="numeric">{{ format(spending.totalPurchases, { currency }) }}</span>
        </div>
      </template>
    </BaseCard>

    <BaseCard title="Largest expenses" subtitle="The five biggest single purchases" :padded="false">
      <BaseEmptyState v-if="!spending.topExpenses.length" icon="bag" title="Nothing to show" compact />

      <ol v-else class="top">
        <li
          v-for="(expense, index) in spending.topExpenses"
          :key="`${expense.productName}-${index}`"
          class="top__item"
          :class="{ 'top__item--first': index === 0 }"
        >
          <span class="top__rank numeric">{{ index + 1 }}</span>

          <div class="top__body">
            <span class="top__name">{{ expense.productName }}</span>
            <span class="top__meta">
              <template v-if="expense.storeName">{{ expense.storeName }} · </template>
              {{ formatDateTime(expense.occurredAt, { withYear: false }) }} · {{ expense.category }} ·
              {{ METHOD_LABEL[expense.paymentMethod] ?? expense.paymentMethod }}
            </span>
          </div>

          <span class="top__amount numeric">{{ format(expense.amount, { currency }) }}</span>
        </li>
      </ol>
    </BaseCard>

    <!-- Reported but deliberately outside the net result: whatever this cash buys
         is already counted as a Cash purchase. -->
    <p v-if="spending.totalCashWithdrawn > 0" class="cash">
      <BaseBadge variant="warning" size="sm">Cash</BaseBadge>
      {{ format(spending.totalCashWithdrawn, { currency }) }} withdrawn this month. It left your
      accounts but is not subtracted again in the net result — cash purchases already cover it.
    </p>
  </div>
</template>

<style scoped lang="scss">
.spending { display: flex; flex-direction: column; gap: var(--sp-4); }

.categories { display: flex; flex-direction: column; }

.category {
  display: flex;
  flex-direction: column;
  gap: var(--sp-2);
  padding: var(--sp-3) var(--sp-5);
  border-bottom: var(--border-subtle);

  &:last-child { border-bottom: none; }
}

.category__head {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: var(--sp-3);
}

.category__name { font-weight: var(--fw-medium); }
.category__total { font-weight: var(--fw-semibold); }

.category__bar {
  height: 6px;
  background: var(--canvas-alt);
  border: var(--border-subtle);
  border-radius: var(--radius-sm);
  overflow: hidden;
}

.category__fill { height: 100%; background: var(--accent); }

.category__meta {
  display: flex;
  justify-content: space-between;
  font-size: var(--fs-xs);
  color: var(--text-muted);
}

.spending__footer {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  font-size: var(--fs-sm);
  color: var(--text-muted);

  .numeric { font-size: var(--fs-md); font-weight: var(--fw-semibold); color: var(--text); }
}

.top { display: flex; flex-direction: column; }

.top__item {
  display: flex;
  align-items: center;
  gap: var(--sp-3);
  padding: var(--sp-3) var(--sp-5);
  border-bottom: var(--border-subtle);

  &:last-child { border-bottom: none; }

  /* The biggest expense is the one worth noticing */
  &--first {
    background: var(--canvas-alt);
    border-left: 3px solid var(--gold);
  }
}

.top__rank {
  display: grid;
  place-items: center;
  width: 24px;
  height: 24px;
  flex: none;

  border: var(--border-subtle);
  border-radius: 50%;
  font-size: var(--fs-xs);
  color: var(--text-muted);
}

.top__item--first .top__rank {
  background: var(--gold);
  border-color: var(--gold);
  color: var(--ink);
  font-weight: var(--fw-semibold);
}

.top__body { flex: 1; display: flex; flex-direction: column; min-width: 0; }
.top__name { font-weight: var(--fw-medium); @include truncate; }
.top__meta { font-size: var(--fs-xs); color: var(--text-muted); }
.top__amount { font-weight: var(--fw-semibold); }

.cash {
  display: flex;
  align-items: baseline;
  flex-wrap: wrap;
  gap: var(--sp-2);

  padding: var(--sp-3) var(--sp-4);
  background: var(--canvas-alt);
  border: var(--border-subtle);
  border-radius: var(--radius);
  font-size: var(--fs-sm);
  color: var(--text-muted);
  line-height: 1.5;
}

@media (max-width: 767px) {
  .category, .top__item { padding-left: var(--sp-4); padding-right: var(--sp-4); }
}
</style>
