import client from './client'

/**
 * Freelance work: agreed once, then delivered and paid whenever those actually
 * happen. Every transition is its own call rather than a status field on an
 * update, because delivering and being paid are different events and only one of
 * them moves money.
 */
export const freelanceJobsApi = {
  list: () => client.get('/freelance-jobs'),
  get: (id) => client.get(`/freelance-jobs/${id}`),

  create: (payload) => client.post('/freelance-jobs', payload),

  /** Refused once the work is paid — delete and re-record instead. */
  update: (id, payload) => client.put(`/freelance-jobs/${id}`, payload),

  /** Finished and handed over. Moves no money. */
  markDelivered: (id, deliveredOn) =>
    client.post(`/freelance-jobs/${id}/delivered`, { deliveredOn }),

  /**
   * The client paid. Deposits into the chosen account and writes a movement, so
   * the money becomes ordinary available balance from this moment on.
   */
  markPaid: (id, payload) => client.post(`/freelance-jobs/${id}/paid`, payload),

  /** Called off. Keeps the record; `remove` is what erases it. */
  cancel: (id, cancelledOn) => client.post(`/freelance-jobs/${id}/cancel`, { cancelledOn }),

  /** Reverses the deposit if the work had been paid. */
  remove: (id) => client.delete(`/freelance-jobs/${id}`)
}

export const FREELANCE_STATUS_VARIANT = {
  InProgress: 'neutral',
  Delivered: 'warning',
  Paid: 'positive',
  Cancelled: 'neutral'
}
