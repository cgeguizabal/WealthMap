import client from './client'

/**
 * The signed-in user's own account. There is no route that names anyone else —
 * the server reads the id from the token.
 */
export const usersApi = {
  me: () => client.get('/users/me'),

  /** Name, country and reporting currency. Email is not editable. */
  updateMe: (payload) => client.put('/users/me', payload),

  /**
   * Replaces the password and ends every session, this one included, so the
   * caller has to sign in again afterwards. That is the point: a password is
   * usually changed because a session is believed to be compromised, and the
   * refresh token an intruder holds does not care what the password is.
   */
  changePassword: (currentPassword, newPassword) =>
    client.post('/users/me/password', { currentPassword, newPassword })
}
