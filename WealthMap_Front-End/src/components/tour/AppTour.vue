<script setup>
import { ref, computed, watch, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import { useTourStore } from '@/stores/tour.store'
import { useI18n } from '@/composables/useI18n'
import { prefersReducedMotion } from '@/composables/useMotionSafe'
import BaseButton from '@/components/base/BaseButton.vue'

/**
 * The spotlight and the card. Rendered once, inside the app shell.
 *
 * The dimming is done with one element: a box positioned over the target with a
 * very large spread shadow, so everything outside it darkens and the target
 * stays untouched. Four masking panels around the target would be the other way,
 * and would need re-measuring on every scroll rather than one translation.
 */
const SPOT_PADDING = 6
const CARD_WIDTH = 320
const CARD_GAP = 12
const VIEWPORT_MARGIN = 12

const tour = useTourStore()
const route = useRoute()
const { t } = useI18n()

/** Null for a step with no target — the card centres itself instead. */
const rect = ref(null)

function measure() {
  const selector = tour.step?.target

  if (!selector) {
    rect.value = null
    return
  }

  const element = document.querySelector(selector)

  if (!element) {
    // The target vanished mid-tour — a list emptied, a panel collapsed. Falling
    // back to a centred card keeps the text readable instead of ending abruptly.
    rect.value = null
    return
  }

  const box = element.getBoundingClientRect()

  rect.value = {
    top: box.top - SPOT_PADDING,
    left: box.left - SPOT_PADDING,
    width: box.width + SPOT_PADDING * 2,
    height: box.height + SPOT_PADDING * 2
  }
}

const spotStyle = computed(() => {
  if (!rect.value) return { display: 'none' }

  return {
    top: `${rect.value.top}px`,
    left: `${rect.value.left}px`,
    width: `${rect.value.width}px`,
    height: `${rect.value.height}px`
  }
})

/**
 * Below the target when there is room, above when there is not, centred when
 * there is no target at all. Clamped to the viewport on both axes so the card is
 * never half off-screen — which is the failure people actually hit, on a narrow
 * phone with a target near an edge.
 */
const cardStyle = computed(() => {
  if (!rect.value) {
    return { top: '50%', left: '50%', transform: 'translate(-50%, -50%)' }
  }

  const below = rect.value.top + rect.value.height + CARD_GAP
  const spaceBelow = window.innerHeight - below
  const fitsBelow = spaceBelow > 200

  const top = fitsBelow ? below : Math.max(VIEWPORT_MARGIN, rect.value.top - CARD_GAP - 200)

  const preferred = rect.value.left + rect.value.width / 2 - CARD_WIDTH / 2
  const maxLeft = window.innerWidth - CARD_WIDTH - VIEWPORT_MARGIN
  const left = Math.min(Math.max(VIEWPORT_MARGIN, preferred), Math.max(VIEWPORT_MARGIN, maxLeft))

  return { top: `${top}px`, left: `${left}px` }
})

const title = computed(() =>
  tour.step ? t(`tour.${tour.activeTour}.${tour.step.key}.title`) : ''
)

const body = computed(() =>
  tour.step ? t(`tour.${tour.activeTour}.${tour.step.key}.body`) : ''
)

/** Brings the target into view before measuring, or the spotlight lands off-screen. */
async function focusStep() {
  const selector = tour.step?.target

  if (selector) {
    document.querySelector(selector)?.scrollIntoView({
      behavior: prefersReducedMotion() ? 'auto' : 'smooth',
      block: 'center'
    })

    // Smooth scrolling has no completion event. Measuring immediately would
    // capture the pre-scroll position, so this waits roughly one scroll.
    await new Promise((resolve) => setTimeout(resolve, prefersReducedMotion() ? 0 : 320))
  }

  await nextTick()
  measure()
}

watch(() => tour.step, focusStep, { immediate: true })

/**
 * Offers the tour when a module is opened for the first time.
 *
 * The delay is for content that arrives after the route does: most of these
 * screens fetch, and a tour that starts before the data lands would find none of
 * its targets and drop every step but the intro.
 */
let startTimer = null

watch(
  () => route.name,
  (name) => {
    clearTimeout(startTimer)
    if (tour.isRunning) tour.stop()

    startTimer = setTimeout(() => tour.startIfUnseen(name), 700)
  },
  { immediate: true }
)

function onKeydown(event) {
  if (!tour.isRunning) return

  if (event.key === 'Escape') tour.stop()
  if (event.key === 'ArrowRight') tour.next()
  if (event.key === 'ArrowLeft') tour.back()
}

onMounted(() => {
  window.addEventListener('resize', measure)
  // Capture phase: the scrolling element is the shell's content column, not the
  // window, so a bubbling listener on window would never hear it.
  window.addEventListener('scroll', measure, true)
  window.addEventListener('keydown', onKeydown)
})

onBeforeUnmount(() => {
  clearTimeout(startTimer)
  window.removeEventListener('resize', measure)
  window.removeEventListener('scroll', measure, true)
  window.removeEventListener('keydown', onKeydown)
})
</script>

<template>
  <Teleport to="body">
    <div v-if="tour.isRunning" class="tour">
      <!-- Swallows clicks so the tour cannot be half-dismissed by tapping the
           page behind it. The spotlight itself is inert. -->
      <div class="tour__blocker" @click="tour.stop()" />

      <div class="tour__spot" :style="spotStyle" />

      <div
        class="tour__card"
        :style="cardStyle"
        role="dialog"
        aria-modal="true"
        :aria-label="title"
      >
        <p class="tour__progress">{{ tour.index + 1 }} / {{ tour.total }}</p>

        <h2 class="tour__title">{{ title }}</h2>
        <p class="tour__body">{{ body }}</p>

        <div class="tour__actions">
          <button class="tour__skip" type="button" @click="tour.stop()">
            {{ t('tour.skip') }}
          </button>

          <div class="tour__nav">
            <BaseButton v-if="!tour.isFirst" size="sm" variant="secondary" @click="tour.back()">
              {{ t('tour.back') }}
            </BaseButton>

            <BaseButton size="sm" variant="primary" @click="tour.next()">
              {{ tour.isLast ? t('tour.done') : t('tour.next') }}
            </BaseButton>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style scoped lang="scss" src="@/assets/styles/components/AppTour.scss"></style>
