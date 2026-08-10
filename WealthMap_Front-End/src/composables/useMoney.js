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

  return { currency, format, formatSigned, formatPercent, toneOf }
}
