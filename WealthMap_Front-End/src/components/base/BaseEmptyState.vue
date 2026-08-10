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

<style scoped lang="scss">
.empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: var(--sp-12) var(--sp-6);
  gap: var(--sp-2);
}

.empty--compact { padding: var(--sp-8) var(--sp-4); }

.empty__icon {
  display: grid;
  place-items: center;
  width: 48px;
  height: 48px;
  margin-bottom: var(--sp-2);

  border: var(--border);
  border-radius: var(--radius);
  background: var(--canvas-alt);
  color: var(--text-muted);
}

.empty__title { font-size: var(--fs-md); font-weight: var(--fw-semibold); }

.empty__message {
  font-size: var(--fs-sm);
  color: var(--text-muted);
  max-width: 42ch;
}

.empty__action { margin-top: var(--sp-3); }
</style>
