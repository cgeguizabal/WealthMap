/**
 * Every currency WealthMap offers, in one place.
 *
 * This list used to be copy-pasted into eight components, which meant adding a
 * currency was eight edits and forgetting one was silent — the code would simply
 * be missing from that one form. There is no reason for the account form and the
 * goal form to disagree about which currencies exist.
 *
 * Order is deliberate: the currencies most likely to be picked first, then the
 * rest alphabetically-ish. `BaseSelect` renders them in this order.
 */
export const CURRENCY_CODES = [
  'USD',
  'GTQ',
  'MXN',
  'EUR',
  'GBP',
  'CAD',
  'BRL',
  'COP',
  'ARS'
]

/** The `{ value, label }` shape every `BaseSelect` in the app expects. */
export const CURRENCY_OPTIONS = CURRENCY_CODES.map((code) => ({
  value: code,
  label: code
}))
