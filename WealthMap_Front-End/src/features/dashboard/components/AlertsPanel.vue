<script setup>
import { computed } from 'vue'
import { motion } from 'motion-v'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'

const props = defineProps({
  alerts: { type: Array, default: () => [] }
})

const ICON_BY_SEVERITY = { Critical: 'alert', Warning: 'alert', Info: 'info' }
const VARIANT_BY_SEVERITY = { Critical: 'negative', Warning: 'warning', Info: 'neutral' }

/** The API already orders Critical → Warning → Info; this only limits the noise. */
const visible = computed(() => props.alerts.slice(0, 6))
const overflow = computed(() => Math.max(0, props.alerts.length - visible.value.length))
</script>

<template>
  <section v-if="alerts.length" class="alerts">
    <header class="alerts__head">
      <h2 class="alerts__title">Needs attention</h2>
      <BaseBadge size="sm">{{ alerts.length }}</BaseBadge>
    </header>

    <ul class="alerts__list">
      <motion.li
        v-for="(alert, index) in visible"
        :key="`${alert.type}-${alert.relatedEntityId ?? index}`"
        :class="['alert', `alert--${alert.severity.toLowerCase()}`]"
        :initial="{ opacity: 0, x: -6 }"
        :animate="{ opacity: 1, x: 0 }"
        :transition="{ duration: 0.22, delay: index * 0.04, ease: [0.2, 0, 0, 1] }"
      >
        <BaseIcon :name="ICON_BY_SEVERITY[alert.severity] ?? 'info'" :size="17" class="alert__icon" />

        <div class="alert__body">
          <p class="alert__title">{{ alert.title }}</p>
          <p class="alert__message">{{ alert.message }}</p>
        </div>

        <BaseBadge :variant="VARIANT_BY_SEVERITY[alert.severity]" size="sm" class="alert__severity">
          {{ alert.severity }}
        </BaseBadge>
      </motion.li>
    </ul>

    <p v-if="overflow" class="alerts__more">
      and {{ overflow }} more — see notifications
    </p>
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
  padding: var(--sp-4) var(--sp-5);
  border-bottom: var(--border-subtle);
}

.alerts__title { font-size: var(--fs-md); font-weight: var(--fw-semibold); }

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
</style>
