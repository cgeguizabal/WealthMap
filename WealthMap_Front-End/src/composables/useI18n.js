import { storeToRefs } from 'pinia'
import { useLocaleStore } from '@/stores/locale.store'
import { SUPPORTED_LOCALES } from '@/i18n'

/**
 * The one import a component needs for copy: `const { t } = useI18n()`.
 *
 * Wrapping the store keeps call sites from reaching into Pinia directly, so the
 * underlying implementation can change without touching seventy components.
 */
export function useI18n() {
  const store = useLocaleStore()
  const { locale } = storeToRefs(store)

  return {
    t: store.t,
    locale,
    setLocale: store.setLocale,
    locales: SUPPORTED_LOCALES
  }
}
