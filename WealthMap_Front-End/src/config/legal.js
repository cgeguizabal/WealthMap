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

/**
 * Both documents are drafts until a lawyer has been through them. The banner
 * that says so is rendered from this flag, not hard-coded into the views, so
 * removing it after review is a one-line change in one place.
 */
export const LEGAL_DOCS_ARE_DRAFT = true

export const LEGAL_ROUTES = {
  privacy: '/privacy',
  terms: '/terms'
}
