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
          and {{ overflow }} more — see notifications
        </p>
      </div>
    </div>
  </section>
</template>

<style scoped lang="scss">
.alerts {
  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
  overflow: hidden;
}

.alerts__head {
  display: flex;
  align-items: center;
  gap: var(--sp-2);
  width: 100%;

  padding: var(--sp-4) var(--sp-5);
  border: none;
  border-bottom: var(--border-subtle);
  background: transparent;
  text-align: left;
  cursor: pointer;

  @include focus-ring;

  &:hover { background: var(--canvas-alt); }
}

.alerts__title { font-size: var(--fs-md); font-weight: var(--fw-semibold); }
.alerts__spacer { flex: 1; }

.alerts__chevron {
  color: var(--text-muted);
  transition: transform var(--dur) var(--ease);

  &.is-open { transform: rotate(180deg); }
}

/* Animating grid rows from 0fr to 1fr collapses to the content's real height
   without hardcoding one. */
.alerts__collapse {
  display: grid;
  grid-template-rows: 0fr;
  transition: grid-template-rows var(--dur) var(--ease);

  &.is-open { grid-template-rows: 1fr; }
}

.alerts__collapse-inner { overflow: hidden; }

.alerts__list { display: flex; flex-direction: column; }

.alert {
  display: flex;
  align-items: flex-start;
  gap: var(--sp-3);
  padding: var(--sp-3) var(--sp-5);
  border-bottom: var(--border-subtle);

  &:last-child { border-bottom: none; }
}

.alert--critical { border-left: 3px solid var(--negative); }
.alert--warning { border-left: 3px solid var(--warning); }
.alert--info { border-left: 3px solid var(--line); }

.alert--critical .alert__icon { color: var(--negative); }
.alert--warning .alert__icon { color: var(--warning); }
.alert--info .alert__icon { color: var(--text-muted); }

.alert__icon { margin-top: 2px; flex: none; }
.alert__body { flex: 1; min-width: 0; }
.alert__title { font-size: var(--fs-sm); font-weight: var(--fw-semibold); }
.alert__message { font-size: var(--fs-sm); color: var(--text-muted); line-height: 1.5; }
.alert__severity { flex: none; }

.alerts__more {
  padding: var(--sp-3) var(--sp-5);
  background: var(--canvas-alt);
  font-size: var(--fs-xs);
  color: var(--text-muted);
}

@media (max-width: 767px) {
  .alert { padding: var(--sp-3) var(--sp-4); flex-wrap: wrap; }
  .alert__severity { display: none; }
  .alerts__head, .alerts__more { padding-left: var(--sp-4); padding-right: var(--sp-4); }
}

@media (prefers-reduced-motion: reduce) {
  .alerts__collapse, .alerts__chevron { transition: none; }
}
</style>
