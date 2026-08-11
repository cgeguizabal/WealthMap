import { defineStore } from 'pinia'
import { ref, watch } from 'vue'
import { detectLocale, isSupported, translate, STORAGE_KEY } from '@/i18n'

/**
 * The chosen language. A store rather than a module-level ref so every component
 * re-renders when it changes — switching language must not need a page reload.
 *
 * Deliberately not part of auth.store: the choice belongs to the browser, not
 * the account, so it survives logging out and applies on the login screen too.
 */
export const useLocaleStore = defineStore('locale', () => {
  const locale = ref(detectLocale())

  function setLocale(next) {
    if (!isSupported(next) || next === locale.value) return
    locale.value = next
  }

  /** `t` reads `locale.value`, so anything using it re-renders on a change. */
  function t(key, params) {
    return translate(locale.value, key, params)
  }

  watch(
    locale,
    (value) => {
      // Screen readers and browser translation prompts both key off this.
      document.documentElement.setAttribute('lang', value)

      try {
        localStorage.setItem(STORAGE_KEY, value)
      } catch {
        // Persisting is a convenience; the session still works without it.
      }
    },
    { immediate: true }
  )

  return { locale, setLocale, t }
})
