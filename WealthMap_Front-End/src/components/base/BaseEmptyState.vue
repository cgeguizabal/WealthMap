<script setup>
import BaseIcon from './BaseIcon.vue'

defineProps({
  icon: { type: String, default: 'info' },
  title: { type: String, required: true },
  message: { type: String, default: '' },
  compact: { type: Boolean, default: false }
})
</script>

<template>
  <div :class="['empty', { 'empty--compact': compact }]">
    <div class="empty__icon">
      <slot name="icon"><BaseIcon :name="icon" :size="compact ? 20 : 26" /></slot>
    </div>

    <h4 class="empty__title">{{ title }}</h4>
    <p v-if="message" class="empty__message">{{ message }}</p>

    <div v-if="$slots.action" class="empty__action">
      <slot name="action" />
    </div>
  </div>
</template>

<style scoped lang="scss" src="./BaseEmptyState.scss"></style>
