import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { TOURS, hasTour, readSeen, writeSeen } from '@/config/tours'
import { currentUser } from '@/api/session'

/**
 * Runs the guided tours and remembers which have been seen.
 *
 * The store owns *which* step is showing; AppTour owns where it is drawn. That
 * split is what keeps the positioning maths — scroll, resize, clamping — out of
 * the part that decides whether a tour should run at all.
 */
export const useTourStore = defineStore('tour', () => {
  const activeTour = ref(null)
  const steps = ref([])
  const index = ref(0)

  const step = computed(() => steps.value[index.value] ?? null)
  const isRunning = computed(() => activeTour.value !== null && steps.value.length > 0)
  const isFirst = computed(() => index.value === 0)
  const isLast = computed(() => index.value >= steps.value.length - 1)
  const total = computed(() => steps.value.length)

  const userId = () => currentUser.value?.id

  /**
   * Keeps only the steps whose target is actually on screen.
   *
   * A tour written for a full module would otherwise point at empty space on a
   * new account — the very user it exists for. Targetless steps always survive,
   * since they are the narration.
   */
  function resolvable(routeName) {
    return (TOURS[routeName] ?? []).filter(
      (candidate) => !candidate.target || document.querySelector(candidate.target)
    )
  }

  function start(routeName) {
    const usable = resolvable(routeName)
    if (usable.length === 0) return false

    activeTour.value = routeName
    steps.value = usable
    index.value = 0

    return true
  }

  /** Starts only if this module has a tour the user has not seen. */
  function startIfUnseen(routeName) {
    if (isRunning.value || !hasTour(routeName)) return false
    if (readSeen(userId())[routeName]) return false

    return start(routeName)
  }

  function markSeen(routeName) {
    const seen = readSeen(userId())
    seen[routeName] = true
    writeSeen(userId(), seen)
  }

  function stop() {
    // Marked seen on *any* exit, including a skip. Someone who dismissed a tour
    // has told you they do not want it; showing it again next visit reads as the
    // app not listening.
    if (activeTour.value) markSeen(activeTour.value)

    activeTour.value = null
    steps.value = []
    index.value = 0
  }

  function next() {
    if (isLast.value) return stop()
    index.value += 1
  }

  function back() {
    if (isFirst.value) return
    index.value -= 1
  }

  /** Clears the record so every tour plays again — the Settings "replay" action. */
  function resetAll() {
    writeSeen(userId(), {})
  }

  return {
    activeTour, step, index, total,
    isRunning, isFirst, isLast,
    start, startIfUnseen, next, back, stop, resetAll
  }
})
