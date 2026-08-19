import { defineStore } from 'pinia'
import { ref, computed, watch, onScopeDispose } from 'vue'
import {
  THEMES,
  THEME_STORAGE_KEY,
  applyTheme,
  isDarkTheme,
  isSupportedTheme,
  readStoredTheme,
  themeColour
} from '@/config/theme'

/**
 * The chosen colour theme.
 *
 * Modelled on the locale store, and for the same reason: the choice belongs to
 * the browser rather than the account, so it survives signing out and applies on
 * the login screen. Putting it on the user row would also mean a round trip
 * before the first paint could be the right colour.
 */
export const useThemeStore = defineStore('theme', () => {
  const theme = ref(readStoredTheme())

  /**
   * Bumped when the OS flips while "system" is selected, so `isDark` recomputes.
   * A media query is not reactive on its own — nothing would tell Vue to look
   * again.
   */
  const systemChanges = ref(0)

  const isDark = computed(() => {
    systemChanges.value
    return isDarkTheme(theme.value)
  })

  function setTheme(next) {
    if (!isSupportedTheme(next) || next === theme.value) return
    theme.value = next
  }

  watch(
    theme,
    (value) => {
      applyTheme(value)

      // Keeps the browser UI in step — the address bar on mobile, and the
      // window chrome of an installed PWA.
      document
        .querySelector('meta[name="theme-color"]')
        ?.setAttribute('content', themeColour(value))

      try {
        localStorage.setItem(THEME_STORAGE_KEY, value)
      } catch {
        // Persisting is a convenience; the session still works without it.
      }
    },
    { immediate: true }
  )

  // Only matters while "system" is selected, but the listener is cheap and
  // always attached — subscribing conditionally would mean tearing it down and
  // rebuilding it on every change of choice, for no gain.
  const query = window.matchMedia?.('(prefers-color-scheme: dark)')

  const onSystemChange = () => {
    systemChanges.value += 1

    if (theme.value === 'system') {
      document
        .querySelector('meta[name="theme-color"]')
        ?.setAttribute('content', themeColour('system'))
    }
  }

  query?.addEventListener('change', onSystemChange)
  onScopeDispose(() => query?.removeEventListener('change', onSystemChange))

  return { theme, isDark, themes: THEMES, setTheme }
})
