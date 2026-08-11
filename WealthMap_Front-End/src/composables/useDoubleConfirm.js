import { useUiStore } from '@/stores/ui.store'

/**
 * Two confirmations in sequence for actions that remove something from view.
 * The first explains what will happen; the second is a deliberate second beat so
 * the action cannot be completed by double-clicking through a single dialog.
 *
 * Resolves true only if both are accepted.
 */
export function useDoubleConfirm() {
  const ui = useUiStore()

  return async function confirmTwice({ title, message, secondMessage, confirmLabel = 'Delete' }) {
    const first = await ui.confirm({
      title,
      message,
      confirmLabel: 'Continue',
      variant: 'danger'
    })

    if (!first) return false

    return ui.confirm({
      title: 'Are you sure you want to proceed?',
      message: secondMessage,
      confirmLabel,
      cancelLabel: 'Keep it',
      variant: 'danger',
      confirmDelayMs: 600
    })
  }
}
