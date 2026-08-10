import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api/auth.api'

export const useAuthStore = defineStore('auth', () => {
  // ── State ────────────────────────────────────
  const token = ref(localStorage.getItem('wm_token'))
  const user = ref(JSON.parse(localStorage.getItem('wm_user') ?? 'null'))
  const loading = ref(false)
  const error = ref(null)

  // ── Getters ──────────────────────────────────
  const isAuthenticated = computed(() => Boolean(token.value))
  const currency = computed(() => user.value?.currency ?? 'USD')
  const initials = computed(() => {
    if (!user.value?.fullName) return '?'

    return user.value.fullName
      .split(' ')
      .slice(0, 2)
      .map((part) => part[0])
      .join('')
      .toUpperCase()
  })

  // ── Actions ──────────────────────────────────
  function persist(response) {
    token.value = response.token
    user.value = {
      id: response.userId,
      email: response.email,
      fullName: response.fullName,
      currency: response.currency ?? 'USD'
    }

    localStorage.setItem('wm_token', token.value)
    localStorage.setItem('wm_user', JSON.stringify(user.value))
  }

  async function login(credentials) {
    loading.value = true
    error.value = null

    try {
      persist(await authApi.login(credentials))
      return true
    } catch (err) {
      error.value = err
      return false
    } finally {
      loading.value = false
    }
  }

  async function register(payload) {
    loading.value = true
    error.value = null

    try {
      persist(await authApi.register(payload))
      return true
    } catch (err) {
      error.value = err
      return false
    } finally {
      loading.value = false
    }
  }

  /**
   * The auth response does not carry the profile currency (see
   * docs/BACKEND_REQUESTS.md #1), so it is set from the value the user typed at
   * registration and corrected from the dashboard response after login.
   */
  function setCurrency(code) {
    if (!code || !user.value || user.value.currency === code) return

    user.value = { ...user.value, currency: code }
    localStorage.setItem('wm_user', JSON.stringify(user.value))
  }

  function logout() {
    token.value = null
    user.value = null
    localStorage.removeItem('wm_token')
    localStorage.removeItem('wm_user')
  }

  return {
    token, user, loading, error,
    isAuthenticated, currency, initials,
    login, register, logout, setCurrency
  }
})