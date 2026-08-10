import client from './client'

export const dashboardApi = {
  /** Aggregates in the user's profile currency; other currencies are listed, not converted. */
  get: () => client.get('/dashboard'),

  /** Computed live server-side, ordered Critical → Warning → Info. Nothing is persisted by this call. */
  alerts: () => client.get('/alerts')
}
