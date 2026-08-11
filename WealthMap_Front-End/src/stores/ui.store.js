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
   *
   * `confirmDelayMs` holds the confirm button disabled for that long after the
   * dialog opens. It exists for confirmations raised immediately after another
   * one: without it a double-click on the first dialog's button lands on the
   * second dialog's button, which appears in the same place.
   */
  function confirm(options = {}) {
    return new Promise((resolve) => {
      confirmState.value = {
        // Left null rather than defaulted to English here: the dialog resolves
        // the fallbacks, so they follow the language selector. A default baked in
        // at this layer would be fixed at whatever language the caller ran in.
        title: options.title ?? null,
        message: options.message ?? '',
        confirmLabel: options.confirmLabel ?? null,
        cancelLabel: options.cancelLabel ?? null,
        variant: options.variant ?? 'primary',
        confirmDelayMs: options.confirmDelayMs ?? 0,
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
