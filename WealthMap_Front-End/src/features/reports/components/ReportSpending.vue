<script setup>
import { useMoney } from '@/composables/useMoney'
import { useDateTime } from '@/composables/useDateTime'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'
import { useI18n } from '@/composables/useI18n'
import { useServerText } from '@/composables/useServerText'

const { t, tc } = useI18n()

defineProps({
  spending: { type: Object, required: true },
  currency: { type: String, required: true }
})

const { format } = useMoney()
const { formatDateTime } = useDateTime()

const { label: serverLabel } = useServerText()
</script>

<template>
  <div class="spending">
    <BaseCard :title="t('reports.spendingByCategory')" :padded="false">
      <BaseEmptyState
        v-if="!spending.byCategory.length"
        icon="bag"
        :title="t('reports.nothingToShow')"
        compact
      />

      <ul v-else class="categories">
        <li v-for="entry in spending.byCategory" :key="entry.category" class="category">
          <div class="category__head">
            <span class="category__name">{{ serverLabel('category', entry.category) }}</span>
            <span class="category__total numeric">
              {{ format(entry.total, { currency }) }}
            </span>
          </div>

          <div class="category__bar" aria-hidden="true">
            <div class="category__fill" :style="{ width: `${entry.sharePercentage}%` }" />
          </div>

          <div class="category__meta">
            <span>{{ tc('composed.itemCount', entry.count) }}</span>
            <span class="numeric">{{ entry.sharePercentage.toFixed(1) }}%</span>
          </div>
        </li>
      </ul>

      <template #footer>
        <div class="spending__footer">
          <span>{{ t('reports.totalSpent') }}</span>
          <span class="numeric">{{ format(spending.totalPurchases, { currency }) }}</span>
        </div>
      </template>
    </BaseCard>

    <BaseCard :title="t('reports.largestExpenses')" :subtitle="t('reports.largestExpensesSubtitle')" :padded="false">
      <BaseEmptyState v-if="!spending.topExpenses.length" icon="bag" :title="t('reports.nothingToShow')" compact />

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
              {{ formatDateTime(expense.occurredAt, { withYear: false }) }} · {{ serverLabel('category', expense.category) }} ·
              {{ serverLabel('paymentMethod', expense.paymentMethod) }}
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

<style scoped lang="scss" src="./ReportSpending.scss"></style>
