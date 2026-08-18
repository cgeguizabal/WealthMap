/**
 * What the app calls itself, and how finished it claims to be.
 *
 * Kept apart from `legal.js` because the two versions move independently: the
 * policy version changes when the documents change, this one when the software
 * does. Conflating them would mean every release re-asked users to accept terms
 * that had not changed.
 */
export const APP_VERSION = '1.0'

/**
 * Drives the "Beta" marker wherever the app names itself.
 *
 * It is a claim about maturity, not a feature flag — nothing behaves
 * differently. Set it to false when the software is no longer beta, and the
 * marker disappears everywhere at once.
 */
export const IS_BETA = true

/** "Beta v1.0", or just "v1.0" once the beta label is dropped. */
export const VERSION_LABEL = IS_BETA ? `Beta v${APP_VERSION}` : `v${APP_VERSION}`
