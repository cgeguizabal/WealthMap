import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { dashboardApi } from '@/api/dashboard.api'
import { useAuthStore } from '@/stores/auth.store'

/**
 * Shared because the dashboard figures are read by more than the dashboard
 * screen, and because re-fetching the user's whole financial picture on every
 * visit to `/` is wasteful when nothing has changed.
 *
 * Cached state that outlives a session is dangerous: it is one person's money.
 * Everything below is therefore tied to the user it was loaded for, and is
 * never served to anyone else.
 */
export const useDashboardStore = defineStore('dashboard', () => {
  const auth = useAuthStore()

  const data = ref(null)
  const alerts = ref([])
  const loading = ref(false)
  const error = ref(null)
  const loadedAt = ref(null)

  /** Which user this data belongs to. Guards against showing it to another. */
  const ownerId = ref(null)
  const stale = ref(false)

  /**
   * Safe to render without re-fetching. Requires data that belongs to the
   * *current* user and has not been invalidated by a mutation elsewhere.
   */
  const isFresh = computed(() =>
    data.value !== null &&
    ownerId.value !== null &&
    ownerId.value === auth.user?.id &&
    !stale.value
  )

  const hasData = computed(() => isFresh.value)
  const criticalAlerts = computed(() => alerts.value.filter((a) => a.severity === 'Critical'))

  /**
   * Dashboard and alerts rebuild the same server-side snapshot (see
   * docs/BACKEND_REQUESTS.md #4). Requesting them together keeps the wall-clock
   * cost at one round trip.
   */
  async function load() {
    loading.value = true
    error.value = null

    try {
      const [dashboard, alertList] = await Promise.all([
        dashboardApi.get(),
        dashboardApi.alerts()
      ])

      data.value = dashboard
      alerts.value = alertList ?? []
      loadedAt.value = new Date()
      ownerId.value = auth.user?.id ?? null
      stale.value = false

      // The auth response omits the profile currency; this is where it is corrected.
      auth.setCurrency(dashboard.currency)

      return dashboard
    } catch (err) {
      error.value = err
      return null
    } finally {
      loading.value = false
    }
  }

  /** Anything that moves money makes these figures wrong until reloaded. */
  function invalidate() {
    stale.value = true
    loadedAt.value = null
  }

  function reset() {
    data.value = null
    alerts.value = []
    error.value = null
    loadedAt.value = null
    ownerId.value = null
    stale.value = false
  }

  return {
    data, alerts, loading, error, loadedAt,
    isFresh, hasData, criticalAlerts,
    load, invalidate, reset
  }
})
