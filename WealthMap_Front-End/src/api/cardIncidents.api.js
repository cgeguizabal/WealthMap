import client from './client'

/**
 * The two kinds of card, as the server names them.
 *
 * Strings rather than the numeric enum: the server answers with these on every
 * DTO, so comparing against them needs no translation table.
 */
export const CARD_KIND = {
  CREDIT: 'CreditCard',
  DEBIT: 'DebitCard'
}

/** Mirrors CardLossReason. Sent as numbers, read back as names. */
export const CARD_LOSS_REASON = {
  LOST: 1,
  STOLEN: 2,
  DAMAGED: 3,
  COMPROMISED: 4
}

/**
 * Reports live under the card they are about, not under a resource of their own:
 * a credit card is a card, and a debit card belongs to an account. One base path
 * per kind is the whole of the difference.
 */
const base = (kind, id) =>
  kind === CARD_KIND.DEBIT ? `/accounts/${id}/debit-card` : `/credit-cards/${id}`

export const cardIncidentsApi = {
  /** Takes the card out of service. `{ reason, reportedOn, notes }`. */
  report: (kind, id, payload) => client.post(`${base(kind, id)}/loss-report`, payload),

  /**
   * Records the replacement. `newLastFour` omitted means the bank reissued the
   * same number — it leaves the recorded one alone rather than clearing it.
   */
  replace: (kind, id, payload) => client.post(`${base(kind, id)}/replacement`, payload),

  /** Closes the report because the card turned up. `{ recoveredOn, notes }`. */
  recover: (kind, id, payload) => client.post(`${base(kind, id)}/recovery`, payload),

  /** Every report ever filed against this card, newest first. */
  list: (kind, id) => client.get(`${base(kind, id)}/incidents`)
}
