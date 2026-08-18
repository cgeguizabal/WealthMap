import axios from 'axios'
import { serverError } from '@/composables/useServerText'
import { accessToken, setAccessToken, clearSession } from './session'

const client = axios.create({
  baseURL: '/api/v1',
  headers: { 'Content-Type': 'application/json' },

  // Sends the refresh cookie. It is httpOnly, so this is the only way it travels
  // — no code here can read it, which is the point.
  withCredentials: true
})

/**
 * The browser's IANA zone, e.g. "America/Guatemala".
 *
 * Sent on every request because the server stores everything in UTC and has no
 * other way to know what day it is where you are. Computing in UTC as well is
 * not the day-out it sounds like: "the next time the 20th comes around" answers
 * next month if the server's date has already rolled over and yours has not, so
 * a card due today reported as due in thirty days.
 */
const browserTimeZone = () => {
  try {
    return Intl.DateTimeFormat().resolvedOptions().timeZone || null
  } catch {
    // Ancient or locked-down runtime. The server falls back to UTC.
    return null
  }
}

// Resolved once: it cannot change without the page being reloaded.
const TIME_ZONE = browserTimeZone()

// ── Request: attach the token and the caller's time zone ──
client.interceptors.request.use((config) => {
  if (accessToken.value) {
    config.headers.Authorization = `Bearer ${accessToken.value}`
  }

  if (TIME_ZONE) {
    config.headers['X-Time-Zone'] = TIME_ZONE
  }

  return config
})

/**
 * Endpoints that must never trigger a refresh. `/auth/refresh` answering 401 is
 * the signal that the session is over; retrying it would recurse. Login and
 * register answer 401 for bad credentials, which a refresh cannot fix.
 */
const NO_REFRESH = ['/auth/refresh', '/auth/login', '/auth/register', '/auth/logout']

/**
 * One refresh at a time. A dashboard load fires several requests at once, and if
 * the access token expired they will all 401 together. Without this, each would
 * start its own refresh: the first rotates the token, the rest present the one it
 * just replaced, and the server reads that as a replay and ends every session.
 * Sharing the promise means the others wait for the same answer.
 */
let refreshInFlight = null

function refreshSession() {
  refreshInFlight ??= client
    .post('/auth/refresh')
    .then((result) => {
      setAccessToken(result.token)
      return result
    })
    .finally(() => {
      refreshInFlight = null
    })

  return refreshInFlight
}

/**
 * Pages that must survive an expired session.
 *
 * Kept as paths rather than route names because this module cannot import the
 * router — the router imports the auth store, which imports the api modules,
 * which import this file. Duplicating four strings is the cheaper half of that
 * trade, and they are checked against the router's routes by a comment there.
 */
const PUBLIC_PATHS = ['/login', '/register', '/privacy', '/terms']

function endSession() {
  clearSession()

  // Bouncing to login from a public page is wrong twice over: the legal
  // documents are meant to be readable by someone with no account at all, and
  // a stale token in localStorage would otherwise make them unreachable in a
  // fresh tab. Signing out is still correct — the redirect is not.
  if (PUBLIC_PATHS.some((path) => window.location.pathname.startsWith(path))) {
    return
  }

  window.location.href = '/login'
}

// ── Response: refresh once, then normalize errors ────────
client.interceptors.response.use(
  (response) => response.data,
  async (error) => {
    const status = error.response?.status
    const request = error.config

    const canRefresh =
      status === 401 &&
      request &&
      // Retried at most once. A second 401 on the replay means the new token is
      // not the problem, so refreshing again would only loop.
      !request.hasRetried &&
      !NO_REFRESH.some((path) => request.url?.startsWith(path))

    if (canRefresh) {
      request.hasRetried = true

      try {
        await refreshSession()

        // Replayed through the instance, so the request interceptor attaches the
        // new token and the response is unwrapped exactly as it would have been.
        return await client(request)
      } catch {
        endSession()
        return Promise.reject(normalizeError(401, error.response?.data))
      }
    }

    if (status === 401) {
      endSession()
    }

    return Promise.reject(normalizeError(status, error.response?.data))
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
