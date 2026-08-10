import { ref, onMounted, onUnmounted } from 'vue'

/**
 * Reactive `matchMedia`. Used where a breakpoint changes *behaviour* rather than
 * only appearance — CSS handles the rest.
 */
export function useMediaQuery(query) {
  const matches = ref(false)
  let mediaQuery = null

  function update(event) {
    matches.value = event.matches
  }

  onMounted(() => {
    mediaQuery = window.matchMedia(query)
    matches.value = mediaQuery.matches
    mediaQuery.addEventListener('change', update)
  })

  onUnmounted(() => {
    mediaQuery?.removeEventListener('change', update)
  })

  return matches
}

/** The width at which the sidebar stops being a drawer and becomes a column. */
export const DESKTOP_QUERY = '(min-width: 1024px)'
