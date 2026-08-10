import client from './client'

/**
 * A shared catalogue: everyone reads every store, only the creator may edit
 * theirs. There is no delete — purchases reference stores.
 */
export const storesApi = {
  list: () => client.get('/stores'),
  get: (id) => client.get(`/stores/${id}`),
  create: (payload) => client.post('/stores', payload),
  update: (id, payload) => client.put(`/stores/${id}`, payload)
}

export const STORE_CATEGORIES = [
  'Groceries', 'Restaurants', 'Electronics', 'Clothing', 'Health',
  'Transport', 'Entertainment', 'Home', 'Services', 'Other'
]
