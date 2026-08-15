import client from './client'

export const bankDefaultsApi = {
  list: () => client.get('/bank-defaults'),

  /**
   * Upsert on (bankName, direction). PUT because it is idempotent: saving the
   * same bank and direction twice replaces the account rather than creating a
   * second, contradictory row.
   */
  save: (payload) => client.put('/bank-defaults', payload),

  /** A real delete — a bank default holds no history worth keeping. */
  remove: (id) => client.delete(`/bank-defaults/${id}`)
}
