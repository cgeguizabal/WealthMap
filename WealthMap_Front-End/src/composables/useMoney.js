import { computed } from 'vue'
import { useAuthStore } from '@/stores/auth.store'

const formatters = new Map()

function formatterFor(currency, options) {
  const key = `${currency}|${options.compact}|${options.decimals}`

  if (!formatters.has(key)) {
    formatters.set(key, new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency,
      notation: options.compact ? 'compact' : 'standard',
      minimumFractionDigits: options.decimals,
      maximumFractionDigits: options.decimals
    }))
  }

  return formatters.get(key)
}

/**
 * Rounds to cents the same way the backend's Money does: ties go away from zero,
 * so 418.525 is 418.53 rather than the 418.52 banker's rounding would give.
 *
 * Exported outside the composable so plain modules can use it without Pinia.
 *
 * The toPrecision step is not decoration. Binary floating point cannot hold most
 * decimal fractions, so some ties land just below the halfway point once scaled:
 * `2.675 * 100` evaluates to 267.49999999999994, and a naive Math.round would
 * round it *down* to 2.67 and quietly disagree with the server. Trimming to 12
 * significant digits restores the value the user actually typed before the tie
 * is judged. (Not every tie is affected — 418.525 * 100 is exactly 41852.5 —
 * which is why the bug shows up only on some amounts.)
 */
export function roundCents(value) {
  const amount = Number(value)
  if (!Number.isFinite(amount)) return 0

  const sign = amount < 0 ? -1 : 1
  const scaled = Number((Math.abs(amount) * 100).toPrecision(12))

  return (sign * Math.round(scaled)) / 100
}

/**
 * Every monetary value in the UI goes through here, so the user's profile
 * currency is applied in exactly one place. Amounts arrive from the API as
 * plain numbers already rounded to cents; this only decides presentation.
 */
export function useMoney() {
  const auth = useAuthStore()
  const currency = computed(() => auth.currency)

  /** `null`/`undefined` render as an em dash rather than "$0.00", which would be a lie. */
  function format(amount, { currency: code, compact = false, decimals = 2 } = {}) {
    if (amount === null || amount === undefined || Number.isNaN(Number(amount))) return '—'

    return formatterFor(code ?? currency.value, { compact, decimals }).format(Number(amount))
  }

  /** Always shows a sign, for deltas where direction is the point. */
  function formatSigned(amount, options = {}) {
    if (amount === null || amount === undefined) return '—'

    const formatted = format(Math.abs(amount), options)
    return Number(amount) < 0 ? `−${formatted}` : `+${formatted}`
  }

  function formatPercent(value, decimals = 1) {
    if (value === null || value === undefined) return '—'

    return `${Number(value).toFixed(decimals)}%`
  }

  /** Semantic class for money that can be either direction. */
  function toneOf(amount) {
    if (amount === null || amount === undefined) return 'neutral'
    if (Number(amount) > 0) return 'positive'
    if (Number(amount) < 0) return 'negative'
    return 'neutral'
  }

  return { currency, format, formatSigned, formatPercent, toneOf, roundCents }
}
