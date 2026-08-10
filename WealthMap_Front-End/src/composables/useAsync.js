import { ref } from 'vue'

/**
 * Wraps one async call with the three states every screen needs: loading, data,
 * error. `run` resolves to the data or `null` — it never throws — so callers can
 * branch on the result instead of wrapping every call site in try/catch.
 *
 * Used instead of a Pinia store wherever state belongs to a single view.
 */
export function useAsync(fn, { immediate = false, initialData = null } = {}) {
  const data = ref(initialData)
  const error = ref(null)
  const loading = ref(false)

  async function run(...args) {
    loading.value = true
    error.value = null

    try {
      data.value = await fn(...args)
      return data.value
    } catch (err) {
      error.value = err
      return null
    } finally {
      loading.value = false
    }
  }

  function reset() {
    data.value = initialData
    error.value = null
    loading.value = false
  }

  if (immediate) run()

  return { data, error, loading, run, reset }
}
