import { translate, MESSAGES, DEFAULT_LOCALE } from '@/i18n'
import { useLocaleStore } from '@/stores/locale.store'
import { useMoney } from '@/composables/useMoney'
import { useDateTime } from '@/composables/useDateTime'

/**
 * Renders an alert or notification from the parts the API sends with it.
 *
 * The server still composes an English `title` and `message`; those are the
 * fallback. When it also sends `params`, the sentence is rebuilt from the
 * translation for its `type` — which is the only way the figures survive being
 * said in another language.
 *
 * Money and dates are formatted here rather than by the server, so they match
 * every other figure on screen: the amount carries the user's currency
 * formatting and the date reads the way dates read in their locale.
 */

/** Params that are amounts, and the param naming the currency they are in. */
const MONEY_PARAMS = ['amount', 'owed', 'checking', 'shortfall', 'obligations', 'income', 'spent']
const DATE_PARAMS = ['dueDate']

function hasKey(locale, key) {
  const walk = (node, path) =>
    path.split('.').reduce((n, part) => (n == null ? undefined : n[part]), node)

  return typeof walk(MESSAGES[locale] ?? MESSAGES[DEFAULT_LOCALE], key) === 'string'
}

export function useAlertText() {
  const localeStore = useLocaleStore()
  const { format } = useMoney()
  const { formatDate } = useDateTime()

  /**
   * Formats the raw parts for display. The server sends invariant decimals and
   * ISO dates precisely so this step can be locale-aware.
   */
  function present(params) {
    const currency = params.currency || undefined
    const out = { ...params }

    for (const key of MONEY_PARAMS) {
      if (out[key] != null && out[key] !== '') {
        out[key] = format(Number(out[key]), currency ? { currency } : {})
      }
    }

    for (const key of DATE_PARAMS) {
      if (out[key]) out[key] = formatDate(out[key])
    }

    return out
  }

  /**
   * `alert` is an AlertDto or a NotificationDto — both carry type, title,
   * message and params.
   */
  function render(alert) {
    const locale = localeStore.locale
    const params = alert?.params ?? {}
    const hasParams = Object.keys(params).length > 0

    // No parts, or a type this build does not know: the server's English is the
    // best available answer, and saying it is better than saying nothing.
    if (!alert?.type || !hasParams || !hasKey(locale, `alert.${alert.type}.title`)) {
      return { title: alert?.title ?? '', message: alert?.message ?? '' }
    }

    const shown = present(params)
    let message = translate(locale, `alert.${alert.type}.message`, shown)

    // One alert ends with a suggestion that depends on a decision the server
    // made; it sends the decision, not the sentence.
    if (alert.type === 'InsufficientBalanceForCardPayment') {
      const suffix = params.savingsCover === 'true' ? 'canCover' : 'cannotCover'
      message += translate(locale, `alert.${alert.type}.${suffix}`, shown)
    }

    return {
      title: translate(locale, `alert.${alert.type}.title`, shown),
      message
    }
  }

  return { render }
}
