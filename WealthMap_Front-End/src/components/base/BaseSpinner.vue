<script setup>
defineProps({
  size: { type: [Number, String], default: 16 },
  label: { type: String, default: 'Loading' }
})
</script>

<template>
  <span class="spinner" :style="{ width: `${size}px`, height: `${size}px` }" role="status">
    <span class="sr-only">{{ label }}</span>
  </span>
</template>

<style scoped lang="scss" src="@/assets/styles/components/BaseSpinner.scss"></style>
