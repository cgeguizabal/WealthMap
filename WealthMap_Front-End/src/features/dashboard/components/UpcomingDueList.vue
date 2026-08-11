<script setup>
import { useMoney } from '@/composables/useMoney'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'
import { useI18n } from '@/composables/useI18n'
import { useServerText } from '@/composables/useServerText'

const { t } = useI18n()
const { label: serverLabel } = useServerText()

defineProps({
  items: { type: Array, default: () => [] }
})

const { format } = useMoney()

const ICON_BY_KIND = { CreditCard: 'card', Debt: 'debt', Installment: 'layers' }

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
  <BaseCard :title="t('dashboard.upcoming')" :subtitle="t('dashboard.next30Days')" :padded="false">
    <BaseEmptyState
      v-if="items.length === 0"
      icon="check-circle"
      :title="t('dashboard.nothingDue')"
      :message="t('dashboard.nothingDueMessage')"
      compact
    />

    <ul v-else class="due">
      <li v-for="item in items" :key="`${item.kind}-${item.entityId}-${item.dueDate}`" class="due__item">
        <span :class="['due__marker', `due__marker--${toneFor(item.daysUntil)}`]" aria-hidden="true" />

        <BaseIcon :name="ICON_BY_KIND[item.kind] ?? 'clock'" :size="16" class="due__icon" />

        <div class="due__body">
          <p class="due__name">{{ item.name }}</p>
          <p class="due__meta">
            {{ serverLabel('dueKind', item.kind) }} · {{ item.dueDate }}
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

<style scoped lang="scss" src="./UpcomingDueList.scss"></style>
