import { useUiStore } from '@/stores/ui.store'
import { useI18n } from '@/composables/useI18n'

/**
 * Two confirmations in sequence for actions that remove something from view.
 * The first explains what will happen; the second is a deliberate second beat so
 * the action cannot be completed by double-clicking through a single dialog.
 *
 * Resolves true only if both are accepted.
 */
export function useDoubleConfirm() {
  const ui = useUiStore()
  const { t } = useI18n()

  return async function confirmTwice({ title, message, secondMessage, confirmLabel }) {
    const first = await ui.confirm({
      title,
      message,
      confirmLabel: t('common.continueLabel'),
      variant: 'danger'
    })

    if (!first) return false

    return ui.confirm({
      title: t('common.proceedQuestion'),
      message: secondMessage,
      // Resolved here rather than as a parameter default, so it follows the
      // language selector instead of freezing at import time.
      confirmLabel: confirmLabel ?? t('common.delete'),
      cancelLabel: t('common.keepIt'),
      variant: 'danger',
      confirmDelayMs: 600
    })
  }
}
