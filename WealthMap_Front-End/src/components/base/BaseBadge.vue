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

<style scoped lang="scss" src="./BaseBadge.scss"></style>
