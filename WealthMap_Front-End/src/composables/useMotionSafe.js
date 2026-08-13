/**
 * motion-v animates in JavaScript, so the CSS `prefers-reduced-motion` rule
 * cannot reach it. These presets collapse to "already at the destination" when
 * the user has asked for reduced motion, instead of every component repeating
 * the check.
 */
const prefersReduced = () =>
  typeof window !== 'undefined' &&
  window.matchMedia?.('(prefers-reduced-motion: reduce)').matches

/**
 * Movement and opacity are timed separately on purpose. The element settles into
 * place in half a second while the fade runs for more than twice that, so the
 * entrance lands early and finishes softly rather than stopping dead. Each
 * preset applies MOVE to whichever axis it travels on — `y` for the page
 * entrance, `x` for list rows.
 */
const DELAY = 0.1
const MOVE = { duration: 0.5, ease: 'easeInOut' }
const FADE = { duration: 1.2, ease: 'easeOut' }

// A fresh object per call: these are handed to motion-v, not kept.
const still = () => ({ initial: { opacity: 1 }, animate: { opacity: 1 }, transition: { duration: 0 } })

/** Entrance for page sections and card grids. */
export function fadeUp({ delay = 0, distance = 8 } = {}) {
  if (prefersReduced()) return still()

  return {
    initial: { opacity: 0, y: distance },
    animate: { opacity: 1, y: 0 },
    transition: { delay: DELAY + delay, y: MOVE, opacity: FADE }
  }
}

/** Entrance for list rows, staggered by index but capped so long lists stay snappy. */
export function fadeInRow(index, { step = 0.04, max = 0.24 } = {}) {
  if (prefersReduced()) return still()

  return {
    initial: { opacity: 0, x: -6 },
    animate: { opacity: 1, x: 0 },
    transition: { delay: DELAY + Math.min(index * step, max), x: MOVE, opacity: FADE }
  }
}
