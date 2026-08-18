import client from './client'

export const creditCardsApi = {
  /** Archived cards are excluded unless asked for. */
  list: ({ includeArchived = false } = {}) =>
    client.get('/credit-cards', { params: includeArchived ? { includeArchived: true } : undefined }),
  get: (id) => client.get(`/credit-cards/${id}`),

  create: (payload) => client.post('/credit-cards', payload),

  /** Limit and used credit are not editable here — the limit has its own endpoint. */
  update: (id, payload) => client.put(`/credit-cards/${id}`, payload),

  /**
   * Archives rather than destroys: the card leaves every list and total, but its
   * purchases, installment plans and payments are preserved.
   */
  remove: (id) => client.delete(`/credit-cards/${id}`),

  /** Undoes an archive. What makes archiving safe to offer rather than final. */
  restore: (id) => client.post(`/credit-cards/${id}/restore`),

  /** Rejected if the new limit falls below what is currently owed. */
  updateLimit: (id, newLimit) => client.put(`/credit-cards/${id}/limit`, { newLimit }),

  /** `sourceType` is a string both ways: 'Account' | 'External'. */
  pay: (id, payload) => client.post(`/credit-cards/${id}/payments`, payload),

  payments: (id) => client.get(`/credit-cards/${id}/payments`),

  /**
   * Identifying digits and tracking mode. Separate from `update` because the two
   * constrain each other and the server writes them in the order that keeps the
   * invariant satisfied.
   */
  updateTracking: (id, payload) => client.put(`/credit-cards/${id}/tracking`, payload)
}
