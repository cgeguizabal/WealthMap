import { translate } from '@/i18n'
import { useLocaleStore } from '@/stores/locale.store'

/**
 * Translates the strings the API sends in English.
 *
 * Two different problems live here, solved differently.
 *
 * `serverLabel` covers enum names and catalogue categories. Those are closed
 * sets, looked up by the exact server value, and are exact.
 *
 * `serverError` covers domain error prose, which is not a closed set: the server
 * composes it with names and amounts inside. It is matched by shape and
 * re-rendered from the captured parts. Anything unrecognised falls through to
 * the server's own wording — worse than a translation, but far better than
 * hiding what actually went wrong.
 *
 * Both are plain functions rather than composable-only, because the axios
 * interceptor has to translate errors and has no component context.
 */

/** Ordered: the first match wins. Captures the parts that carry meaning. */
const ERROR_PATTERNS = [
  {
    key: 'insufficientFunds',
    // "Insufficient funds in 'Main'. Available: 0.00 USD, requested: 128.40 USD."
    test: /^Insufficient funds in '(.+?)'\.\s*Available:\s*(.+?),\s*requested:\s*(.+?)\.?$/i,
    map: (m) => ({ name: m[1], available: m[2], requested: m[3] })
  },
  {
    key: 'blockedAccount',
    test: /^Account '(.+?)' is blocked for saving/i,
    map: (m) => ({ name: m[1] })
  },
  {
    key: 'exceedsCredit',
    test: /exceeds the available credit on '(.+?)'/i,
    map: (m) => ({ name: m[1] })
  },
  { key: 'alreadyArchived', test: /is already archived/i, map: () => ({}) },
  { key: 'futureDate', test: /cannot be in the future/i, map: () => ({}) },
  {
    key: 'currencyMismatch',
    test: /currenc(?:y|ies).*(?:mismatch|do not match|cannot mix|no conversion)/i,
    map: () => ({})
  }
]

/** Whole categories of failure the API names in its `title`. */
const TITLE_KEYS = {
  'Validation failed': 'validationFailed',
  'Business rule violation': 'businessRule',
  'Not found': 'notFound'
}

function currentLocale() {
  // Reading the store rather than useI18n so this works from plain modules.
  return useLocaleStore().locale
}

/**
 * A server enum or category. Falls back to the raw value, so a member added to
 * the API before it is added here still reads as something meaningful.
 */
export function serverLabel(group, value) {
  if (value == null || value === '') return ''

  const key = `server.${group}.${value}`
  const resolved = translate(currentLocale(), key)

  return resolved === key ? String(value) : resolved
}

export function serverError(message) {
  const locale = currentLocale()

  if (!message) return translate(locale, 'common.somethingWentWrong')

  const titleKey = TITLE_KEYS[message]
  if (titleKey) return translate(locale, `serverMessage.${titleKey}`)

  for (const pattern of ERROR_PATTERNS) {
    const match = message.match(pattern.test)
    if (match) return translate(locale, `serverMessage.${pattern.key}`, pattern.map(match))
  }

  return message
}

/** Component-facing wrapper, so templates read `serverLabel('category', x)`. */
export function useServerText() {
  return { label: serverLabel, error: serverError }
}
