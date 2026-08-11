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

<style scoped lang="scss" src="./BaseProgress.scss"></style>
