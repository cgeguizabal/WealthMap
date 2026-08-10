import client from './client'

export const notificationsApi = {
  /** `{ unreadOnly?, page?, pageSize? }` → paged envelope */
  list: (params = {}) => client.get('/notifications', { params }),

  /** Persists currently-true alerts, skipping any already unread. Returns only what it created. */
  sync: () => client.post('/notifications/sync'),

  markRead: (id) => client.post(`/notifications/${id}/read`)
}
