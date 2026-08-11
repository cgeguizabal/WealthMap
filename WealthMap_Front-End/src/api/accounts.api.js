import client from './client'

export const accountsApi = {
  list: () => client.get('/accounts'),
  get: (id) => client.get(`/accounts/${id}`),

  /** `type` is an integer: 1 Checking, 2 Savings. Currency is explicit here. */
  create: (payload) => client.post('/accounts', payload),

  /** Only name, bankName and notes are editable — balance moves via movements. */
  update: (id, payload) => client.put(`/accounts/${id}`, payload),

  /**
   * Archives rather than destroys: the account leaves every list and total, but
   * its movements and the purchases and payments referencing it are preserved.
   */
  remove: (id) => client.delete(`/accounts/${id}`),

  block: (id) => client.post(`/accounts/${id}/block`),
  unblock: (id) => client.post(`/accounts/${id}/unblock`),

  /** `type` restricted to 2 (Deposit) or 3 (Bonus); amount is in the account's currency. */
  deposit: (id, payload) => client.post(`/accounts/${id}/deposit`, payload),

  /** Always recorded as an ATM withdrawal; `location` is optional. */
  withdraw: (id, payload) => client.post(`/accounts/${id}/withdraw`, payload),

  transfer: (payload) => client.post('/accounts/transfer', payload),

  movements: (id, params = {}) => client.get(`/accounts/${id}/movements`, { params })
}

export const ACCOUNT_TYPE = { CHECKING: 1, SAVINGS: 2 }

export const ACCOUNT_TYPE_OPTIONS = [
  { value: ACCOUNT_TYPE.CHECKING, label: 'Checking' },
  { value: ACCOUNT_TYPE.SAVINGS, label: 'Savings' }
]

/** Only these two inbound types may be created by hand; the rest are system-generated. */
export const DEPOSIT_TYPE_OPTIONS = [
  { value: 2, label: 'Deposit' },
  { value: 3, label: 'Bonus' }
]
