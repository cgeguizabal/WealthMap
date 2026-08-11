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

<style scoped lang="scss" src="@/assets/styles/components/BaseSkeleton.scss"></style>
