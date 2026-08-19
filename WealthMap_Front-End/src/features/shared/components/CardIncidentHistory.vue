<script setup>
import { computed } from 'vue'
import { useDateTime } from '@/composables/useDateTime'
import { useI18n } from '@/composables/useI18n'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'

/**
 * What happened to this card, and when.
 *
 * Shown only once there is something to show. A card that has never been lost has
 * no history worth a heading, and an empty state here would suggest the user had
 * failed to record something.
 */
const { t } = useI18n()
const { formatDate } = useDateTime()

const props = defineProps({
  incidents: { type: Array, default: () => [] }
})

const REASONS = {
  Lost: 'cardLoss.reasonLost',
  Stolen: 'cardLoss.reasonStolen',
  Damaged: 'cardLoss.reasonDamaged',
  Compromised: 'cardLoss.reasonCompromised'
}

const rows = computed(() =>
  props.incidents.map((incident) => ({
    ...incident,
    reasonLabel: t(REASONS[incident.reason] ?? 'cardLoss.reasonLost'),
    variant: incident.status === 'Open' ? 'warning' : 'neutral',
    statusLabel: t(`cardLoss.status${incident.status}`)
  }))
)
</script>

<template>
  <BaseCard v-if="rows.length" :title="t('cardLoss.historyTitle')">
    <ol class="history">
      <li v-for="incident in rows" :key="incident.id" class="history__item">
        <div class="history__head">
          <span class="history__reason">{{ incident.reasonLabel }}</span>
          <BaseBadge :variant="incident.variant">{{ incident.statusLabel }}</BaseBadge>
        </div>

        <p class="history__line">
          {{ t('cardLoss.reportedOnDate', { date: formatDate(incident.reportedOn) }) }}
          <span v-if="incident.lastFourAtReport" class="numeric">
            · ••••{{ incident.lastFourAtReport }}
          </span>
        </p>

        <p v-if="incident.replacedOn" class="history__line">
          {{ t('cardLoss.replacedOnDate', { date: formatDate(incident.replacedOn) }) }}
          <span v-if="incident.newLastFour" class="numeric">
            · ••••{{ incident.newLastFour }}
          </span>
          <!-- Said explicitly, because an absent number here means "the bank
               reissued the same one", not "nobody wrote it down". -->
          <span v-else class="history__note">· {{ t('cardLoss.sameNumber') }}</span>
        </p>

        <p v-if="incident.recoveredOn" class="history__line">
          {{ t('cardLoss.recoveredOnDate', { date: formatDate(incident.recoveredOn) }) }}
        </p>

        <p v-if="incident.notes" class="history__notes">{{ incident.notes }}</p>
      </li>
    </ol>
  </BaseCard>
</template>

<style scoped lang="scss" src="@/assets/styles/features/shared/CardIncidentHistory.scss"></style>
