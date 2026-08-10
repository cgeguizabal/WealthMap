<script setup>
/**
 * A placeholder shaped like the content that will replace it, so the layout
 * does not jump when data lands. Prefer this to a spinner wherever the final
 * shape is predictable.
 */
defineProps({
  height: { type: [Number, String], default: 16 },
  width: { type: String, default: '100%' },
  radius: { type: String, default: 'var(--radius-sm)' },
  /** Renders several stacked bars, the last one shortened like a paragraph. */
  lines: { type: Number, default: 1 }
})
</script>

<template>
  <div v-if="lines > 1" class="skeleton-stack" aria-hidden="true">
    <span
      v-for="line in lines"
      :key="line"
      class="skeleton"
      :style="{
        height: typeof height === 'number' ? `${height}px` : height,
        width: line === lines ? '60%' : width,
        borderRadius: radius
      }"
    />
  </div>

  <span
    v-else
    class="skeleton"
    aria-hidden="true"
    :style="{
      height: typeof height === 'number' ? `${height}px` : height,
      width,
      borderRadius: radius
    }"
  />
</template>

<style scoped lang="scss">
.skeleton {
  display: block;
  background: linear-gradient(
    90deg,
    var(--canvas-alt) 25%,
    var(--line) 50%,
    var(--canvas-alt) 75%
  );
  background-size: 200% 100%;
  animation: shimmer 1.4s infinite;
}

.skeleton-stack {
  display: flex;
  flex-direction: column;
  gap: var(--sp-2);
}

@keyframes shimmer {
  to { background-position: -200% 0; }
}

@media (prefers-reduced-motion: reduce) {
  .skeleton { animation: none; background: var(--canvas-alt); }
}
</style>
