import client from './client'

export const authApi = {
  register: (payload) => client.post('/auth/register', payload),
  login: (payload) => client.post('/auth/login', payload),

  /**
   * Ends the session server-side and clears the refresh cookie. Takes no body —
   * the token is in the cookie the browser attaches.
   */
  logout: (allSessions = false) => client.post(`/auth/logout?allSessions=${allSessions}`)
}
