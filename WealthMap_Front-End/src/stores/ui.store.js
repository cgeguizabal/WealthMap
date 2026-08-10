import { defineStore } from 'pinia'
import { ref } from 'vue'

let nextId = 1

/**
 * Cross-cutting UI state: toasts and the global confirm dialog. These live in a
 * store rather than a composable because any component may raise them while a
 * single component near the app root renders them.
 */
export const useUiStore = defineStore('ui', () => {
  const toasts = ref([])
  const confirmState = ref(null)

  function pushToast({ type = 'info', title = '', message = '', timeout = 4500 }) {
    const id = nextId++

    toasts.value.push({ id, type, title, message })

    if (timeout > 0) {
      setTimeout(() => dismissToast(id), timeout)
    }

    return id
  }

  function dismissToast(id) {
    toasts.value = toasts.value.filter((toast) => toast.id !== id)
  }

  /**
   * Opens the confirm dialog and resolves to true/false when the user answers,
   * so callers can `if (await confirm(...))` instead of wiring callbacks.
   */
  function confirm(options = {}) {
    return new Promise((resolve) => {
      confirmState.value = {
        title: options.title ?? 'Are you sure?',
        message: options.message ?? '',
        confirmLabel: options.confirmLabel ?? 'Confirm',
        cancelLabel: options.cancelLabel ?? 'Cancel',
        variant: options.variant ?? 'primary',
        resolve
      }
    })
  }

  function resolveConfirm(answer) {
    confirmState.value?.resolve(answer)
    confirmState.value = null
  }

  return { toasts, confirmState, pushToast, dismissToast, confirm, resolveConfirm }
})
