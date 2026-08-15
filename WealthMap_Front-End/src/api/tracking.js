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
