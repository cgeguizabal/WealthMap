<script setup>
import { computed } from 'vue'

const props = defineProps({
  value: { type: Number, default: 0 },
  max: { type: Number, default: 100 },
  variant: {
    type: String,
    default: 'accent',
    validator: (v) => ['accent', 'positive', 'negative', 'warning'].includes(v)
  },
  size: { type: String, default: 'md', validator: (v) => ['sm', 'md'].includes(v) },
  label: { type: String, default: '' }
})

/** Clamped so an over-funded goal or over-limit card cannot overflow the track. */
const percent = computed(() => {
  if (!props.max) return 0
  return Math.min(100, Math.max(0, (props.value / props.max) * 100))
})
</script>

<template>
  <div class="progress">
    <div v-if="label || $slots.label" class="progress__head">
      <slot name="label"><span class="progress__label">{{ label }}</span></slot>
      <span class="progress__value numeric">{{ percent.toFixed(0) }}%</span>
    </div>

    <div
      :class="['progress__track', `progress__track--${size}`]"
      role="progressbar"
      :aria-valuenow="Math.round(percent)"
      aria-valuemin="0"
      aria-valuemax="100"
    >
      <div :class="['progress__bar', `progress__bar--${variant}`]" :style="{ width: `${percent}%` }" />
    </div>
  </div>
</template>

<style scoped lang="scss">
.progress { display: flex; flex-direction: column; gap: var(--sp-2); }

.progress__head {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: var(--sp-2);
}

.progress__label { font-size: var(--fs-sm); color: var(--text-muted); }
.progress__value { font-size: var(--fs-sm); font-weight: var(--fw-semibold); }

.progress__track {
  background: var(--canvas-alt);
  border: var(--border);
  border-radius: var(--radius-sm);
  overflow: hidden;
}

.progress__track--sm { height: 8px; }
.progress__track--md { height: 12px; }

.progress__bar {
  height: 100%;
  transition: width var(--dur) var(--ease);
}

.progress__bar--accent   { background: var(--accent); }
.progress__bar--positive { background: var(--positive); }
.progress__bar--negative { background: var(--negative); }
.progress__bar--warning  { background: var(--warning); }

@media (prefers-reduced-motion: reduce) {
  .progress__bar { transition: none; }
}
</style>
