<script setup>
import BaseSpinner from './BaseSpinner.vue'

defineProps({
  variant: {
    type: String,
    default: 'primary',
    validator: (v) => ['primary', 'secondary', 'ghost', 'danger'].includes(v)
  },
  size: {
    type: String,
    default: 'md',
    validator: (v) => ['sm', 'md', 'lg'].includes(v)
  },
  type: { type: String, default: 'button' },
  loading: { type: Boolean, default: false },
  disabled: { type: Boolean, default: false },
  block: { type: Boolean, default: false }
})

defineEmits(['click'])
</script>

<template>
  <button
    :type="type"
    :class="['btn', `btn--${variant}`, `btn--${size}`, { 'btn--block': block, 'btn--loading': loading }]"
    :disabled="disabled || loading"
    :aria-busy="loading"
    @click="$emit('click', $event)"
  >
    <BaseSpinner v-if="loading" :size="size === 'sm' ? 12 : 14" class="btn__spinner" />
    <span v-else-if="$slots.icon" class="btn__icon"><slot name="icon" /></span>

    <span class="btn__label"><slot /></span>
  </button>
</template>

<style scoped lang="scss" src="@/assets/styles/components/BaseButton.scss"></style>
