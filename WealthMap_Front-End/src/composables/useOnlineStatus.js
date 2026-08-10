import { ref, onMounted, onUnmounted } from 'vue'

/**
 * Financial data is never served from cache, so going offline means requests
 * fail rather than returning something stale. The UI has to say so plainly.
 */
export function useOnlineStatus() {
  const isOnline = ref(true)

  function goOnline() { isOnline.value = true }
  function goOffline() { isOnline.value = false }

  onMounted(() => {
    isOnline.value = navigator.onLine
    window.addEventListener('online', goOnline)
    window.addEventListener('offline', goOffline)
  })

  onUnmounted(() => {
    window.removeEventListener('online', goOnline)
    window.removeEventListener('offline', goOffline)
  })

  return { isOnline }
}
