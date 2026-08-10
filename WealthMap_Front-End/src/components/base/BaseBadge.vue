<script setup>
defineProps({
  variant: {
    type: String,
    default: 'neutral',
    validator: (v) => ['neutral', 'positive', 'negative', 'warning', 'accent'].includes(v)
  },
  size: { type: String, default: 'md', validator: (v) => ['sm', 'md'].includes(v) }
})
</script>

<template>
  <span :class="['badge', `badge--${variant}`, `badge--${size}`]">
    <slot />
  </span>
</template>

<style scoped lang="scss">
.badge {
  display: inline-flex;
  align-items: center;
  gap: var(--sp-1);

  border: 1px solid;
  border-radius: var(--radius-sm);
  font-weight: var(--fw-medium);
  line-height: 1;
  white-space: nowrap;
}

.badge--sm { padding: 3px var(--sp-2); font-size: var(--fs-xs); }
.badge--md { padding: 4px var(--sp-2); font-size: var(--fs-sm); }

.badge--neutral  { background: var(--canvas-alt);    border-color: var(--line);     color: var(--text-muted); }
.badge--positive { background: var(--positive-soft); border-color: var(--positive); color: var(--positive); }
.badge--negative { background: var(--negative-soft); border-color: var(--negative); color: var(--negative); }
.badge--warning  { background: var(--warning-soft);  border-color: var(--warning);  color: var(--warning); }
.badge--accent   { background: #E6E9EF;              border-color: var(--accent);   color: var(--accent); }
</style>
