import client from './client'

export const creditCardsApi = {
  list: () => client.get('/credit-cards'),
  get: (id) => client.get(`/credit-cards/${id}`),

  create: (payload) => client.post('/credit-cards', payload),

  /** Limit and used credit are not editable here — the limit has its own endpoint. */
  update: (id, payload) => client.put(`/credit-cards/${id}`, payload),

  /** Rejected if the new limit falls below what is currently owed. */
  updateLimit: (id, newLimit) => client.put(`/credit-cards/${id}/limit`, { newLimit }),

  /** `sourceType` is a string both ways: 'Account' | 'External'. */
  pay: (id, payload) => client.post(`/credit-cards/${id}/payments`, payload),

  payments: (id) => client.get(`/credit-cards/${id}/payments`)
}
