/**
 * motion-v animates in JavaScript, so the CSS `prefers-reduced-motion` rule
 * cannot reach it. These presets collapse to "already at the destination" when
 * the user has asked for reduced motion, instead of every component repeating
 * the check.
 */
const prefersReduced = () =>
  typeof window !== 'undefined' &&
  window.matchMedia?.('(prefers-reduced-motion: reduce)').matches

const EASE = [0.2, 0, 0, 1]

/** Entrance for page sections and card grids. */
export function fadeUp({ duration = 0.28, delay = 0, distance = 8 } = {}) {
  if (prefersReduced()) {
    return { initial: { opacity: 1 }, animate: { opacity: 1 }, transition: { duration: 0 } }
  }

  return {
    initial: { opacity: 0, y: distance },
    animate: { opacity: 1, y: 0 },
    transition: { duration, delay, ease: EASE }
  }
}

/** Entrance for list rows, staggered by index but capped so long lists stay snappy. */
export function fadeInRow(index, { duration = 0.22, step = 0.04, max = 0.24 } = {}) {
  if (prefersReduced()) {
    return { initial: { opacity: 1 }, animate: { opacity: 1 }, transition: { duration: 0 } }
  }

  return {
    initial: { opacity: 0, x: -6 },
    animate: { opacity: 1, x: 0 },
    transition: { duration, delay: Math.min(index * step, max), ease: EASE }
  }
}
