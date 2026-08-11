import en from './en'
import es from './es'

/**
 * A small hand-rolled i18n, in the spirit of the rest of the project: two
 * locales and simple {placeholder} interpolation do not need a library, and this
 * stays about seventy lines with no dependency to keep current.
 *
 * If plural rules or per-locale date formats are ever needed, this is the point
 * to swap in vue-i18n — the `t(key, params)` call shape is deliberately the same.
 */
export const MESSAGES = { en, es }

export const SUPPORTED_LOCALES = [
  { value: 'en', labelKey: 'language.english' },
  { value: 'es', labelKey: 'language.spanish' }
]

export const DEFAULT_LOCALE = 'en'

export const STORAGE_KEY = 'wealthmap.locale'

export function isSupported(locale) {
  return Object.prototype.hasOwnProperty.call(MESSAGES, locale)
}

/**
 * Falls back to the browser's language before English, so a Spanish-speaking
 * user gets Spanish on their first visit rather than having to go and find the
 * selector.
 */
export function detectLocale() {
  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    if (stored && isSupported(stored)) return stored
  } catch {
    // Private browsing can throw on access; the browser language still works.
  }

  const preferred = (navigator?.languages ?? [navigator?.language]).filter(Boolean)

  for (const tag of preferred) {
    const base = String(tag).toLowerCase().split('-')[0]
    if (isSupported(base)) return base
  }

  return DEFAULT_LOCALE
}

/** Walks a dotted key. Returns undefined rather than throwing on a bad path. */
function lookup(messages, key) {
  return key.split('.').reduce((node, part) => (node == null ? undefined : node[part]), messages)
}

/**
 * Resolves a key against the locale, falling back to English and finally to the
 * key itself — a missing translation should leave a readable breadcrumb on
 * screen, never a blank or a crash.
 */
export function translate(locale, key, params) {
  const resolved = lookup(MESSAGES[locale] ?? {}, key) ?? lookup(MESSAGES[DEFAULT_LOCALE], key)

  if (typeof resolved !== 'string') {
    if (import.meta.env.DEV) console.warn(`[i18n] missing key: ${key}`)
    return key
  }

  if (!params) return resolved

  return resolved.replace(/\{(\w+)\}/g, (match, name) =>
    Object.prototype.hasOwnProperty.call(params, name) ? String(params[name]) : match
  )
}

/**
 * Reports keys present in one locale but not the other. Runs in dev only: the
 * two files are maintained by hand, and a silently missing Spanish string would
 * otherwise surface as English text in the middle of a Spanish screen.
 */
export function reportMissingKeys() {
  if (!import.meta.env.DEV) return

  const flatten = (node, prefix = '') =>
    Object.entries(node).flatMap(([key, value]) => {
      const path = prefix ? `${prefix}.${key}` : key
      return value && typeof value === 'object' ? flatten(value, path) : [path]
    })

  const enKeys = new Set(flatten(en))
  const esKeys = new Set(flatten(es))

  const missingInEs = [...enKeys].filter((k) => !esKeys.has(k))
  const missingInEn = [...esKeys].filter((k) => !enKeys.has(k))

  if (missingInEs.length) console.warn('[i18n] missing in es:', missingInEs)
  if (missingInEn.length) console.warn('[i18n] missing in en:', missingInEn)
}
