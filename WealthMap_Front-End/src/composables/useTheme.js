import { storeToRefs } from 'pinia'
import { useThemeStore } from '@/stores/theme.store'

/**
 * The one import a component needs for theming, mirroring `useI18n`.
 *
 * Most components need none of this — they read CSS custom properties, which
 * change on their own. This is for the few that must know: the selector itself,
 * and anything drawing to a canvas or picking an image where CSS cannot reach.
 */
export function useTheme() {
  const store = useThemeStore()
  const { theme, isDark } = storeToRefs(store)

  return {
    theme,
    /** True when the browser is actually rendering dark, "system" resolved. */
    isDark,
    setTheme: store.setTheme,
    themes: store.themes
  }
}
