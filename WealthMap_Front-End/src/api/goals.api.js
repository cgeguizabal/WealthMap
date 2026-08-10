import client from './client'

export const savingsGoalsApi = {
  list: () => client.get('/savings-goals'),
  get: (id) => client.get(`/savings-goals/${id}`),
  create: (payload) => client.post('/savings-goals', payload),
  update: (id, payload) => client.put(`/savings-goals/${id}`, payload),
  remove: (id) => client.delete(`/savings-goals/${id}`),

  /**
   * Linked goals require a source account and perform a real transfer.
   * Unlinked goals track only and reject a source account outright.
   */
  contribute: (id, payload) => client.post(`/savings-goals/${id}/contribute`, payload)
}

export const productGoalsApi = {
  list: () => client.get('/product-goals'),
  get: (id) => client.get(`/product-goals/${id}`),
  create: (payload) => client.post('/product-goals', payload),
  update: (id, payload) => client.put(`/product-goals/${id}`, payload),
  remove: (id) => client.delete(`/product-goals/${id}`),

  /** Never touches an account — amount only. */
  contribute: (id, amount) => client.post(`/product-goals/${id}/contribute`, { amount })
}

export const GOAL_STATUS_VARIANT = {
  OnTrack: 'positive',
  BehindSchedule: 'warning',
  DeadlinePassed: 'negative',
  Completed: 'accent'
}

export const GOAL_STATUS_LABEL = {
  OnTrack: 'On track',
  BehindSchedule: 'Behind schedule',
  DeadlinePassed: 'Deadline passed',
  Completed: 'Completed'
}
