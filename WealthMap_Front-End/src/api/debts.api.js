import client from './client'

export const debtsApi = {
  list: () => client.get('/debts'),
  get: (id) => client.get(`/debts/${id}`),

  /** `remainingAmount` defaults to the original; pass less for an already part-paid debt. */
  create: (payload) => client.post('/debts', payload),

  /** Amounts are not editable here — they move through payments. */
  update: (id, payload) => client.put(`/debts/${id}`, payload),

  remove: (id) => client.delete(`/debts/${id}`),

  pay: (id, payload) => client.post(`/debts/${id}/payments`, payload),

  /** Only an Active debt can default; paying a defaulted one reactivates it. */
  markDefaulted: (id) => client.post(`/debts/${id}/default`),

  payments: (id) => client.get(`/debts/${id}/payments`)
}

export const DEBT_STATUS_VARIANT = {
  Active: 'accent',
  PaidOff: 'positive',
  Defaulted: 'negative'
}
