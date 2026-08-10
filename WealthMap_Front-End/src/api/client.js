import axios from 'axios'

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

function normalizeError(status, data) {
  // Field-keyed validation errors → { amount: ['...'], name: ['...'] }
  if (data?.errors) {
    const fields = {}

    for (const [key, messages] of Object.entries(data.errors)) {
      fields[key.charAt(0).toLowerCase() + key.slice(1)] = messages
    }

    return {
      status,
      message: data.title ?? 'Validation failed',
      fields
    }
  }

  return {
    status,
    message: data?.detail ?? data?.title ?? 'Something went wrong.',
    fields: null
  }
}

export default client