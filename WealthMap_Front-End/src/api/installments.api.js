import client from './client'

export const installmentsApi = {
  list: () => client.get('/installment-purchases'),
  get: (id) => client.get(`/installment-purchases/${id}`),

  /** Charges the card the full price immediately — that is how tasa 0 consumes a credit line. */
  create: (payload) => client.post('/installment-purchases', payload),

  /** Takes no installment id: always pays the oldest unpaid row. */
  pay: (id, payload) => client.post(`/installment-purchases/${id}/pay`, payload)
}

/**
 * Mirrors the backend's own split so the schedule can be shown before the plan
 * is created: every instalment is total ÷ months, and the last one absorbs the
 * rounding remainder so the rows sum to exactly the total.
 */
export function previewSchedule(totalPrice, monthsCount) {
  const total = Number(totalPrice)
  const months = Number(monthsCount)

  if (!total || !months || months < 1) return null

  const base = Math.round((total / months) * 100) / 100
  const last = Math.round((total - base * (months - 1)) * 100) / 100

  if (base <= 0 || last <= 0) return null

  return { base, last, months, isEven: base === last }
}
