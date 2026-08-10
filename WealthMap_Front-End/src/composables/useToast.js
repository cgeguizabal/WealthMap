import { useUiStore } from '@/stores/ui.store'

/**
 * Ergonomic wrapper over the UI store's toast queue, so components write
 * `toast.error(err.message)` instead of assembling an object.
 */
export function useToast() {
  const ui = useUiStore()

  return {
    success: (message, title = 'Done') => ui.pushToast({ type: 'success', title, message }),
    error: (message, title = 'Something went wrong') => ui.pushToast({ type: 'error', title, message, timeout: 7000 }),
    warning: (message, title = 'Heads up') => ui.pushToast({ type: 'warning', title, message }),
    info: (message, title = '') => ui.pushToast({ type: 'info', title, message }),
    dismiss: ui.dismissToast
  }
}
