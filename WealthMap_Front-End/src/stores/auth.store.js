import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api/auth.api'
import { accessToken, currentUser, setSession, setUser, clearSession } from '@/api/session'

export const useAuthStore = defineStore('auth', () => {
  // ── State ────────────────────────────────────
  // Token and user live in api/session so the axios interceptor can rotate the
  // token without importing this store — see the note there.
  const token = accessToken
  const user = currentUser
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
    setSession(response.token, {
      id: response.userId,
      email: response.email,
      fullName: response.fullName,
      currency: response.currency ?? 'USD'
    })
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

    setUser({ ...user.value, currency: code })
  }

  /**
   * Revokes the refresh token server-side before dropping local state. The call
   * is allowed to fail — the session must end locally either way, and the token
   * expires on its own — but skipping it would leave a working refresh token
   * alive for two weeks after the user asked to be signed out.
   */
  async function logout({ allSessions = false } = {}) {
    try {
      await authApi.logout(allSessions)
    } catch {
      // Offline, or the token was already gone. Local sign-out still stands.
    } finally {
      clearSession()
    }
  }

  return {
    token, user, loading, error,
    isAuthenticated, currency, initials,
    login, register, logout, setCurrency
  }
})
