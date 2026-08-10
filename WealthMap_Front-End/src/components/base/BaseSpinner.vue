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

<style scoped lang="scss">
.spinner {
  display: inline-block;
  border: 2px solid currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: spin 700ms linear infinite;
  opacity: 0.85;
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip: rect(0 0 0 0);
  white-space: nowrap;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (prefers-reduced-motion: reduce) {
  .spinner { animation-duration: 2s; }
}
</style>
