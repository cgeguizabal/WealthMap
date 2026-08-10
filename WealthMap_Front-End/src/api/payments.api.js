import client from './client'

export const paymentsApi = {
  /**
   * User-wide history across cards, debts and installments.
   * `{ from?, to?, targetType?, page?, pageSize? }` — `to` includes its whole day.
   */
  list: (params = {}) => client.get('/payments', { params })
}

export const PAYMENT_TARGET_OPTIONS = [
  { value: 'CreditCard', label: 'Credit cards' },
  { value: 'Debt', label: 'Debts' },
  { value: 'Installment', label: 'Installments' }
]

export const PAYMENT_SOURCE = { ACCOUNT: 'Account', EXTERNAL: 'External' }
