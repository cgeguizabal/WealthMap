<script setup>
import { storeToRefs } from 'pinia'
import { useUiStore } from '@/stores/ui.store'
import BaseIcon from './BaseIcon.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

/**
 * Mounted once near the app root. Any component raises a toast through the UI
 * store; this is the only place they are rendered.
 */
const ui = useUiStore()
const { toasts } = storeToRefs(ui)

const ICON_BY_TYPE = {
  success: 'check-circle',
  error: 'alert',
  warning: 'alert',
  info: 'info'
}
</script>

<template>
  <Teleport to="body">
    <TransitionGroup name="toast" tag="div" class="toasts" aria-live="polite">
      <div v-for="toast in toasts" :key="toast.id" :class="['toast', `toast--${toast.type}`]" role="status">
        <BaseIcon :name="ICON_BY_TYPE[toast.type] ?? 'info'" :size="18" class="toast__icon" />

        <div class="toast__content">
          <p v-if="toast.title" class="toast__title">{{ toast.title }}</p>
          <p v-if="toast.message" class="toast__message">{{ toast.message }}</p>
        </div>

        <button class="toast__close" type="button" :aria-label="t('common.dismiss')" @click="ui.dismissToast(toast.id)">
          <BaseIcon name="x" :size="14" />
        </button>
      </div>
    </TransitionGroup>
  </Teleport>
</template>

<style scoped lang="scss">
.toasts {
  position: fixed;
  top: var(--sp-4);
  right: var(--sp-4);
  z-index: 200;

  display: flex;
  flex-direction: column;
  gap: var(--sp-2);
  width: min(360px, calc(100vw - var(--sp-8)));
}

.toast {
  display: flex;
  align-items: flex-start;
  gap: var(--sp-3);

  padding: var(--sp-3) var(--sp-4);
  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
}

.toast--success { border-left: 4px solid var(--positive); }
.toast--error   { border-left: 4px solid var(--negative); }
.toast--warning { border-left: 4px solid var(--warning); }
.toast--info    { border-left: 4px solid var(--accent); }

.toast--success .toast__icon { color: var(--positive); }
.toast--error .toast__icon   { color: var(--negative); }
.toast--warning .toast__icon { color: var(--warning); }
.toast--info .toast__icon    { color: var(--accent); }

.toast__icon { margin-top: 1px; }
.toast__content { flex: 1; min-width: 0; }
.toast__title { font-size: var(--fs-sm); font-weight: var(--fw-semibold); }
.toast__message { font-size: var(--fs-sm); color: var(--text-muted); word-break: break-word; }

.toast__close {
  flex: none;
  display: grid;
  place-items: center;
  width: 22px;
  height: 22px;

  border: none;
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--text-muted);
  cursor: pointer;

  @include focus-ring;
  &:hover { background: var(--canvas-alt); color: var(--ink); }
}

.toast-enter-active, .toast-leave-active { transition: all var(--dur) var(--ease); }
.toast-enter-from { opacity: 0; transform: translateX(16px); }
.toast-leave-to { opacity: 0; transform: translateX(16px); }
.toast-move { transition: transform var(--dur) var(--ease); }

@media (max-width: 767px) {
  .toasts { top: auto; bottom: var(--sp-4); left: var(--sp-4); right: var(--sp-4); width: auto; }
  .toast-enter-from, .toast-leave-to { transform: translateY(16px); }
}

@media (prefers-reduced-motion: reduce) {
  .toast-enter-active, .toast-leave-active, .toast-move { transition: none; }
}
</style>
