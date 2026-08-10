import client from './client'

export const purchasesApi = {
  /** `{ year?, month?, category?, page?, pageSize? }` — month requires year. */
  list: (params = {}) => client.get('/purchases', { params }),
  get: (id) => client.get(`/purchases/${id}`),
  create: (payload) => client.post('/purchases', payload)
}

export const PAYMENT_METHOD = { DEBIT: 1, CREDIT: 2, CASH: 3 }

/**
 * Each method requires a different instrument. Currency is inherited from the
 * account or card, so only cash asks for it.
 */
export const PAYMENT_METHOD_OPTIONS = [
  {
    value: PAYMENT_METHOD.DEBIT,
    label: 'Debit',
    icon: 'wallet',
    note: 'Withdraws from an account'
  },
  {
    value: PAYMENT_METHOD.CREDIT,
    label: 'Credit card',
    icon: 'card',
    note: 'Charges a card'
  },
  {
    value: PAYMENT_METHOD.CASH,
    label: 'Cash',
    icon: 'receipt',
    note: 'Records only — cash is untracked'
  }
]

export const PURCHASE_CATEGORIES = [
  'Food', 'Groceries', 'Transport', 'Electronics', 'Clothing', 'Health',
  'Entertainment', 'Home', 'Services', 'Education', 'Travel', 'Other'
]
