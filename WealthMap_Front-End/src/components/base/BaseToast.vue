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

<style scoped lang="scss" src="@/assets/styles/components/BaseToast.scss"></style>
