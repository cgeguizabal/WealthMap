import client from './client'

/** Recurring extras only — one-off money is a Bonus deposit on an account. */
export const incomesApi = {
  list: () => client.get('/additional-incomes'),
  get: (id) => client.get(`/additional-incomes/${id}`),
  create: (payload) => client.post('/additional-incomes', payload),
  update: (id, payload) => client.put(`/additional-incomes/${id}`, payload),
  remove: (id) => client.delete(`/additional-incomes/${id}`)
}

/**
 * `name` is the enum name the API sends back, and is what lookups match on.
 * `label` is display text, replaced per locale in the component — matching on it
 * would break the moment the interface is not in English.
 */
export const INCOME_FREQUENCY_OPTIONS = [
  { value: 1, name: 'Weekly', label: 'Weekly' },
  { value: 2, name: 'Biweekly', label: 'Biweekly' },
  { value: 3, name: 'Monthly', label: 'Monthly' },
  { value: 4, name: 'Yearly', label: 'Yearly' }
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
