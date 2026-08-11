<script setup>
import { ref, computed, useId } from 'vue'
import { motion } from 'motion-v'
import { fadeInRow } from '@/composables/useMotionSafe'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import { useI18n } from '@/composables/useI18n'
import { useServerText } from '@/composables/useServerText'
import { useAlertText } from '@/composables/useAlertText'

const { t } = useI18n()
const { label: serverLabel } = useServerText()
const { render: renderAlert } = useAlertText()

const props = defineProps({
  alerts: { type: Array, default: () => [] }
})

/** Open by default — these are things the user is being asked to act on. */
const expanded = ref(true)
const listId = useId()

const ICON_BY_SEVERITY = { Critical: 'alert', Warning: 'alert', Info: 'info' }
const VARIANT_BY_SEVERITY = { Critical: 'negative', Warning: 'warning', Info: 'neutral' }

/** The API already orders Critical → Warning → Info; this only limits the noise. */
const visible = computed(() => props.alerts.slice(0, 6))
const overflow = computed(() => Math.max(0, props.alerts.length - visible.value.length))
</script>

<template>
  <section v-if="alerts.length" class="alerts">
    <!-- The count stays visible when collapsed, so hiding the list never hides
         the fact that there is something to deal with. -->
    <button
      class="alerts__head"
      type="button"
      :aria-expanded="expanded"
      :aria-controls="listId"
      @click="expanded = !expanded"
    >
      <h2 class="alerts__title">{{ t('dashboard.needsAttention') }}</h2>
      <BaseBadge size="sm">{{ alerts.length }}</BaseBadge>

      <span class="alerts__spacer" />

      <BaseIcon
        name="chevron-down"
        :size="18"
        :class="['alerts__chevron', { 'is-open': expanded }]"
      />
    </button>

    <div :id="listId" :class="['alerts__collapse', { 'is-open': expanded }]">
      <div class="alerts__collapse-inner">
        <ul class="alerts__list">
          <motion.li
            v-for="(alert, index) in visible"
            :key="`${alert.type}-${alert.relatedEntityId ?? index}`"
            :class="['alert', `alert--${alert.severity.toLowerCase()}`]"
            v-bind="fadeInRow(index)"
          >
            <BaseIcon :name="ICON_BY_SEVERITY[alert.severity] ?? 'info'" :size="17" class="alert__icon" />

            <div class="alert__body">
              <!-- Rebuilt from the parts the API sends, so the figures survive
                   translation. Falls back to the server's English sentence. -->
              <p class="alert__title">{{ renderAlert(alert).title }}</p>
              <p class="alert__message">{{ renderAlert(alert).message }}</p>
            </div>

            <BaseBadge :variant="VARIANT_BY_SEVERITY[alert.severity]" size="sm" class="alert__severity">
              {{ serverLabel('severity', alert.severity) }}
            </BaseBadge>
          </motion.li>
        </ul>

        <p v-if="overflow" class="alerts__more">
          {{ t('composed.andMore', { count: overflow }) }}
        </p>
      </div>
    </div>
  </section>
</template>

<style scoped lang="scss" src="./AlertsPanel.scss"></style>
