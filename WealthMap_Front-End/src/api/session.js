import { ref } from 'vue'

/**
 * The stored session, owned here rather than in the auth store.
 *
 * Both the store and the axios interceptor need to read and write it, and the
 * interceptor cannot import the store: the store imports the api modules, which
 * import the client, so the dependency would be a cycle. Keeping the state in a
 * module that imports nothing but Vue breaks it — and gives the rotating access
 * token exactly one place to live, so a refresh updates the header and the UI at
 * the same moment.
 */
const TOKEN_KEY = 'wm_token'
const USER_KEY = 'wm_user'

const readUser = () => {
  try {
    return JSON.parse(localStorage.getItem(USER_KEY) ?? 'null')
  } catch {
    // Corrupt entry: treat it as no session rather than breaking every page.
    return null
  }
}

export const accessToken = ref(localStorage.getItem(TOKEN_KEY))
export const currentUser = ref(readUser())

/** Replaces only the access token, as a refresh does — the user is unchanged. */
export function setAccessToken(token) {
  accessToken.value = token
  localStorage.setItem(TOKEN_KEY, token)
}

export function setSession(token, user) {
  accessToken.value = token
  currentUser.value = user

  localStorage.setItem(TOKEN_KEY, token)
  localStorage.setItem(USER_KEY, JSON.stringify(user))
}

export function setUser(user) {
  currentUser.value = user
  localStorage.setItem(USER_KEY, JSON.stringify(user))
}

export function clearSession() {
  accessToken.value = null
  currentUser.value = null

  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USER_KEY)
}
