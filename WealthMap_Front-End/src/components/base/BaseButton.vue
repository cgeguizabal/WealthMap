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

<style scoped lang="scss">
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--sp-2);

  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow-sm);
  background: var(--surface);
  color: var(--ink);

  font-weight: var(--fw-medium);
  line-height: 1;
  white-space: nowrap;
  cursor: pointer;

  @include pressable;
  @include focus-ring;

  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
    box-shadow: none;
  }

  &:hover:not(:disabled) { background: var(--canvas-alt); }
}

/* ── Sizes ─────────────────────────────────── */
.btn--sm { padding: 0 var(--sp-3); height: 30px; font-size: var(--fs-sm); }
.btn--md { padding: 0 var(--sp-4); height: 36px; font-size: var(--fs-base); }
.btn--lg { padding: 0 var(--sp-5); height: 44px; font-size: var(--fs-md); }

/* ── Variants ──────────────────────────────── */
.btn--primary {
  background: var(--accent);
  color: #fff;
  border-color: var(--ink);

  &:hover:not(:disabled) { background: var(--accent-hover); }
}

.btn--danger {
  background: var(--negative);
  color: #fff;

  &:hover:not(:disabled) { background: #7a332b; }
}

.btn--ghost {
  background: transparent;
  border-color: transparent;
  box-shadow: none;

  &:hover:not(:disabled) { background: var(--canvas-alt); }
  &:active:not(:disabled) { transform: none; box-shadow: none; }
}

.btn--block { width: 100%; }

.btn__icon { display: inline-flex; }
.btn__label { display: inline-flex; align-items: center; }
.btn--loading .btn__label { opacity: 0.85; }
</style>
