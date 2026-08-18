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
 * Movement is a spring and opacity is a tween, which is the split Motion itself
 * recommends. A spring carries momentum, so the element decelerates into place
 * the way a physical object would; opacity has no momentum to model, and a
 * spring on it would overshoot past 1 and clip.
 *
 * `visualDuration` is how long the movement *looks* like it takes — the spring
 * settles for a few more milliseconds after that, but imperceptibly, so this is
 * the number to reason about rather than a raw stiffness/damping pair.
 *
 * The fade is deliberately shorter than the movement: text is fully legible
 * while the element is still settling, rather than the element sitting in place
 * half-transparent waiting for a long fade to catch up.
 */
const MOVE = { type: 'spring', visualDuration: 0.8, bounce: 0.15 }
const MOVE_ROW = { type: 'spring', visualDuration: 0.26, bounce: 0.1 }
const FADE = { duration: 0.2, ease: 'easeOut' }

// A fresh object per call: these are handed to motion-v, not kept.
const still = () => ({ initial: { opacity: 1 }, animate: { opacity: 1 }, transition: { duration: 0 } })

/** Entrance for page sections and card grids. */
export function fadeUp({ delay = 0, distance = 16 } = {}) {
  if (prefersReduced()) return still()

  return {
    initial: { opacity: 0, y: distance },
    animate: { opacity: 1, y: 0 },
    transition: { y: { ...MOVE, delay }, opacity: { ...FADE, delay } }
  }
}

/**
 * Entrance for list rows. Rows rise into place like everything else — a
 * sideways entrance reads as a drawer opening rather than content arriving.
 * The stagger is capped so a long list does not turn into a slow wave.
 */
export function fadeInRow(index, { step = 0.035, max = 0.2, distance = 10 } = {}) {
  if (prefersReduced()) return still()

  const delay = Math.min(index * step, max)

  return {
    initial: { opacity: 0, y: distance },
    animate: { opacity: 1, y: 0 },
    transition: { y: { ...MOVE_ROW, delay }, opacity: { ...FADE, delay } }
  }
}
