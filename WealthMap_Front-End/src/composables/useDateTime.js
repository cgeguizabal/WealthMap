/**
 * One place for reading timestamps off the API.
 *
 * Everything the backend sends is UTC. These render in the viewer's own timezone,
 * which is what "when did I spend this?" actually means — a purchase made at 8pm
 * should read 8pm, not the 02:00 UTC it was stored as.
 */
import { useI18n } from '@/composables/useI18n'

export function useDateTime() {
  const { t, tc } = useI18n()

  /** Matches a bare calendar date, with no time and no zone. */
  const DATE_ONLY = /^\d{4}-\d{2}-\d{2}$/

  function toDate(value) {
    if (!value) return null
    if (value instanceof Date) return Number.isNaN(value.getTime()) ? null : value

    // A date-only string is a calendar date, not an instant. `new Date('2026-08-13')`
    // reads it as UTC midnight, which then displays as the 12th anywhere west of
    // Greenwich — a payment due the 13th would show as due the 12th. Building it
    // from local parts keeps the day the day.
    if (typeof value === 'string' && DATE_ONLY.test(value)) {
      const [year, month, day] = value.split('-').map(Number)
      const local = new Date(year, month - 1, day)
      return Number.isNaN(local.getTime()) ? null : local
    }

    const date = new Date(value)
    return Number.isNaN(date.getTime()) ? null : date
  }

  function formatDate(value, { withYear = true } = {}) {
    const date = toDate(value)
    if (!date) return '—'

    return date.toLocaleDateString(undefined, {
      year: withYear ? 'numeric' : undefined,
      month: 'short',
      day: '2-digit'
    })
  }

  /** 24-hour, because a finance log is scanned rather than read aloud. */
  function formatTime(value) {
    const date = toDate(value)
    if (!date) return ''

    return date.toLocaleTimeString(undefined, {
      hour: '2-digit',
      minute: '2-digit',
      hour12: false
    })
  }

  function formatDateTime(value, options = {}) {
    const date = toDate(value)
    if (!date) return '—'

    return `${formatDate(value, options)}, ${formatTime(value)}`
  }

  /**
   * A value for an <input type="datetime-local">, which has no timezone and so
   * must be built from local parts — toISOString would silently shift it to UTC.
   */
  function toLocalInputValue(value = new Date()) {
    const date = toDate(value)
    if (!date) return ''

    const pad = (n) => String(n).padStart(2, '0')

    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}` +
      `T${pad(date.getHours())}:${pad(date.getMinutes())}`
  }

  /** The inverse: a local datetime-local string back to the UTC instant the API wants. */
  function fromLocalInputValue(value) {
    if (!value) return null
    const date = new Date(value)
    return Number.isNaN(date.getTime()) ? null : date.toISOString()
  }

  /**
   * "in 12 days", "tomorrow", "overdue" — translated.
   *
   * Lives here rather than in the components that need it because it was
   * previously written inline and returned English regardless of locale, which
   * is exactly the kind of string that survives an i18n pass unnoticed.
   */
  function relativeDay(days) {
    if (days < 0) return t('common.overdue')
    if (days === 0) return t('common.today')
    if (days === 1) return t('common.tomorrow')

    return tc('composed.inDays', days)
  }

  return {
    formatDate, formatTime, formatDateTime,
    toLocalInputValue, fromLocalInputValue,
    relativeDay
  }
}
