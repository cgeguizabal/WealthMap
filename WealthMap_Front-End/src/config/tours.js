/**
 * The guided tours, one per module, keyed by route name.
 *
 * Steps point at elements through `data-tour` attributes rather than CSS
 * classes. Classes here are styling, and renaming one to fix a layout should not
 * silently break a tour — a `data-tour` attribute exists for no other reason, so
 * it is obvious what removing it costs.
 *
 * A step whose target is not on screen is dropped when the tour starts rather
 * than shown pointing at nothing. That is what lets one definition serve both an
 * empty module and a full one: the "here is your list" step simply does not
 * appear until there is a list.
 *
 * A step with no target is centred, which is how each tour opens.
 */
export const TOURS = {
  dashboard: [
    { key: 'welcome' },
    { key: 'stats', target: '[data-tour="dashboard-stats"]' },
    { key: 'safeToSpend', target: '[data-tour="dashboard-safe"]' },
    { key: 'alerts', target: '[data-tour="dashboard-alerts"]' }
  ],

  accounts: [
    { key: 'intro' },
    { key: 'add', target: '[data-tour="accounts-add"]' },
    { key: 'list', target: '[data-tour="accounts-list"]' },
    { key: 'moving', target: '[data-tour="accounts-list"]' },
    { key: 'transfer', target: '[data-tour="accounts-transfer"]' }
  ],

  'credit-cards': [
    { key: 'intro' },
    { key: 'add', target: '[data-tour="cards-add"]' },
    { key: 'cutoff', target: '[data-tour="cards-list"]' },
    { key: 'pay', target: '[data-tour="cards-pay"]' },
    { key: 'detail', target: '[data-tour="cards-list"]' }
  ],

  purchases: [
    { key: 'intro' },
    { key: 'add', target: '[data-tour="purchases-add"]' },
    { key: 'method', target: '[data-tour="purchases-add"]' },
    { key: 'list', target: '[data-tour="purchases-list"]' },
    { key: 'fixing', target: '[data-tour="purchases-list"]' }
  ],

  installments: [
    { key: 'intro' },
    { key: 'create' },
    { key: 'list', target: '[data-tour="installments-list"]' }
  ],

  debts: [
    { key: 'intro' },
    { key: 'add', target: '[data-tour="debts-add"]' },
    { key: 'paying' }
  ],

  goals: [
    { key: 'intro' },
    { key: 'kinds', target: '[data-tour="goals-tabs"]' },
    { key: 'add', target: '[data-tour="goals-add"]' },
    { key: 'contribute' }
  ],

  job: [
    { key: 'intro' },
    { key: 'salary', target: '[data-tour="job-salary"]' },
    { key: 'deductions', target: '[data-tour="job-deductions"]' },
    { key: 'deductionKinds', target: '[data-tour="job-deductions"]' },
    { key: 'paydays', target: '[data-tour="job-days"]' },
    { key: 'extraIncome', target: '[data-tour="job-income"]' }
  ],

  reports: [
    { key: 'intro' },
    { key: 'month', target: '[data-tour="reports-month"]' },
    { key: 'download', target: '[data-tour="reports-download"]' }
  ],

  settings: [
    { key: 'intro' },
    { key: 'bankDefaults', target: '[data-tour="settings-defaults"]' },
    { key: 'appearance' },
    { key: 'replay' }
  ]
}

export const hasTour = (routeName) => Boolean(TOURS[routeName]?.length)

/**
 * Which tours a user has already been shown.
 *
 * Keyed by user id, not stored globally: a second person signing in on the same
 * browser is new to the app even if the browser is not. Kept in localStorage
 * rather than on the user row because it is a browser convenience — losing it
 * shows a tour twice, which is not worth a column and a round trip.
 */
export const seenStorageKey = (userId) => `wm_tours_seen_${userId ?? 'anon'}`

export function readSeen(userId) {
  try {
    return JSON.parse(localStorage.getItem(seenStorageKey(userId)) ?? '{}') ?? {}
  } catch {
    return {}
  }
}

export function writeSeen(userId, seen) {
  try {
    localStorage.setItem(seenStorageKey(userId), JSON.stringify(seen))
  } catch {
    // Private browsing. The tour reappears next time, which is the harmless
    // direction to fail in.
  }
}
