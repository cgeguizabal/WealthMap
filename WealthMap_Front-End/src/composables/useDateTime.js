/**
 * One place for reading timestamps off the API.
 *
 * Everything the backend sends is UTC. These render in the viewer's own timezone,
 * which is what "when did I spend this?" actually means — a purchase made at 8pm
 * should read 8pm, not the 02:00 UTC it was stored as.
 */
export function useDateTime() {
  function toDate(value) {
    if (!value) return null
    const date = value instanceof Date ? value : new Date(value)
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

  return { formatDate, formatTime, formatDateTime, toLocalInputValue, fromLocalInputValue }
}
