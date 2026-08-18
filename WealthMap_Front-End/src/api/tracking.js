/**
 * The tracking enum, mirrored from the backend.
 *
 * Requests send the integer and responses return the name, so both directions
 * are needed. Kept here rather than inline in a component because four files
 * refer to it and a stray literal `2` would be unreadable at the call site.
 */
export const TRACKING_MODE = {
  MANUAL: 1,
  EMAIL_SYNC: 2
}

/** Turns the response string back into the integer a request needs. */
export function trackingModeValue(name) {
  return name === 'EmailSync' ? TRACKING_MODE.EMAIL_SYNC : TRACKING_MODE.MANUAL
}

export const TRANSFER_DIRECTION = {
  INBOUND: 1,
  OUTBOUND: 2
}

export function directionValue(name) {
  return name === 'Outbound' ? TRANSFER_DIRECTION.OUTBOUND : TRANSFER_DIRECTION.INBOUND
}

/** Whether a debit card reaches an account, and of what kind. */
export const DEBIT_CARD_TYPE = {
  NONE: 1,
  PHYSICAL: 2,
  DIGITAL: 3
}

export function debitCardTypeValue(name) {
  if (name === 'Physical') return DEBIT_CARD_TYPE.PHYSICAL
  if (name === 'Digital') return DEBIT_CARD_TYPE.DIGITAL
  return DEBIT_CARD_TYPE.NONE
}
