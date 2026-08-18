/**
 * The policy version the app is currently asking people to accept.
 *
 * Registration sends this to the API, which stores it against the user, so the
 * record answers "which text did they agree to" rather than merely "did they
 * agree". Raise it whenever the documents change materially — and raise it in
 * the markdown headers at the same time, or the record will point at a version
 * that never existed.
 */
export const POLICY_VERSION = '1.0'

export const POLICY_EFFECTIVE_DATE = '2026-08-18'

// Both documents are published as part of a beta, not as lawyer-reviewed text.
// The visible draft banner was removed deliberately; legal review is still
// outstanding and tracked in docs/DEPLOYMENT_CHECKLIST.md. The pages carry the
// app's beta marker instead, which says less about who has read them.

/**
 * Where the two documents live. Also duplicated in PUBLIC_PATHS in
 * api/client.js, which cannot import this file without a cycle.
 */
export const LEGAL_ROUTES = {
  privacy: '/privacy',
  terms: '/terms'
}
