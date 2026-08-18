import client from './client'

export const authApi = {
  register: (payload) => client.post('/auth/register', payload),
  login: (payload) => client.post('/auth/login', payload),

  /**
   * Ends the session server-side and clears the refresh cookie. Takes no body —
   * the token is in the cookie the browser attaches.
   */
  logout: (allSessions = false) => client.post(`/auth/logout?allSessions=${allSessions}`),

  /**
   * Erases the account and everything in it. Immediate, and there is no undo.
   *
   * Takes the password as well as the session token: a token lives in the browser
   * and outlives the moment it was issued, so on its own it would let a borrowed
   * laptop destroy someone's records.
   */
  deleteAccount: (password) => client.delete('/auth/me', { data: { password } })
}
