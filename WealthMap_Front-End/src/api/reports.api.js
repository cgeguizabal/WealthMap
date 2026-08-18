import client from './client'

/**
 * The browser's IANA zone, e.g. "America/Guatemala".
 *
 * Sent with every report request because the server stores everything in UTC and
 * has no other way to know where the user's month begins. Without it a purchase
 * made at nine in the evening on the 31st is already the 1st in UTC, and would
 * appear in the next month's report while every screen showed it in this one.
 */
const browserTimeZone = () => {
  try {
    return Intl.DateTimeFormat().resolvedOptions().timeZone || undefined
  } catch {
    // Ancient or locked-down runtime. The server falls back to UTC.
    return undefined
  }
}

export const reportsApi = {
  /** `month` is an ISO year-month, e.g. "2026-08". */
  monthly: (month) =>
    client.get(`/reports/monthly/${month}`, { params: { tz: browserTimeZone() } }),

  /**
   * Returns a Blob — the response interceptor hands back `response.data` either way.
   *
   * `lang` is the language chosen in the app, not the browser's. The PDF is
   * rendered server-side from its own copy of the vocabulary, so it has to be
   * told; without it the document comes back in English however the app reads.
   */
  monthlyPdf: (month, lang) =>
    client.get(`/reports/monthly/${month}/pdf`, {
      responseType: 'blob',
      params: { lang, tz: browserTimeZone() }
    })
}

/** Current year-month in the format the API expects. */
export function currentMonth() {
  const now = new Date()
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`
}

/**
 * Browsers cannot save a response body directly, so the blob is turned into a
 * temporary object URL, clicked, and revoked.
 */
export function downloadBlob(blob, filename) {
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')

  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()

  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}
