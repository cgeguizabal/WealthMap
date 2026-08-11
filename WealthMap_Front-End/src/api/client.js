import axios from 'axios'
import { serverError } from '@/composables/useServerText'

const client = axios.create({
  baseURL: '/api/v1',
  headers: { 'Content-Type': 'application/json' }
})

// ── Request: attach the token ────────────────────────────
client.interceptors.request.use((config) => {
  const token = localStorage.getItem('wm_token')

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

// ── Response: normalize errors, handle 401 ───────────────
client.interceptors.response.use(
  (response) => response.data,
  (error) => {
    const status = error.response?.status
    const data = error.response?.data

    if (status === 401) {
      localStorage.removeItem('wm_token')
      localStorage.removeItem('wm_user')

      if (!window.location.pathname.startsWith('/login')) {
        window.location.href = '/login'
      }
    }

    return Promise.reject(normalizeError(status, data))
  }
)

/**
 * The API answers in English. Translating here rather than at each of the twenty
 * or so `toast.error(err.message)` sites means none of them has to know, and a
 * new one cannot forget.
 *
 * `serverError` returns the server's own wording for anything it does not
 * recognise, so an untranslated message still says what went wrong.
 */
function normalizeError(status, data) {
  // Field-keyed validation errors → { amount: ['...'], name: ['...'] }
  if (data?.errors) {
    const fields = {}

    for (const [key, messages] of Object.entries(data.errors)) {
      fields[key.charAt(0).toLowerCase() + key.slice(1)] = messages
    }

    return {
      status,
      message: serverError(data.title ?? 'Validation failed'),
      // Field messages are left as sent: they name specific rules and lengths
      // that a pattern match would mangle rather than translate.
      fields
    }
  }

  return {
    status,
    message: serverError(data?.detail ?? data?.title),
    fields: null
  }
}

export default client