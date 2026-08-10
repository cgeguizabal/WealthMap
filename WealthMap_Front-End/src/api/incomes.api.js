import client from './client'

/** Recurring extras only — one-off money is a Bonus deposit on an account. */
export const incomesApi = {
  list: () => client.get('/additional-incomes'),
  get: (id) => client.get(`/additional-incomes/${id}`),
  create: (payload) => client.post('/additional-incomes', payload),
  update: (id, payload) => client.put(`/additional-incomes/${id}`, payload),
  remove: (id) => client.delete(`/additional-incomes/${id}`)
}

export const INCOME_FREQUENCY_OPTIONS = [
  { value: 1, label: 'Weekly' },
  { value: 2, label: 'Biweekly' },
  { value: 3, label: 'Monthly' },
  { value: 4, label: 'Yearly' }
]

/** The same normalisation the dashboard applies, for showing a monthly equivalent. */
export function toMonthly(amount, frequency) {
  const value = Number(amount) || 0

  switch (frequency) {
    case 'Weekly': return (value * 52) / 12
    case 'Biweekly': return (value * 26) / 12
    case 'Yearly': return value / 12
    default: return value
  }
}
