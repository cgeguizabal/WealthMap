import client from './client'

export const reportsApi = {
  /** `month` is an ISO year-month, e.g. "2026-08". */
  monthly: (month) => client.get(`/reports/monthly/${month}`),

  /** Returns a Blob — the response interceptor hands back `response.data` either way. */
  monthlyPdf: (month) =>
    client.get(`/reports/monthly/${month}/pdf`, { responseType: 'blob' })
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
