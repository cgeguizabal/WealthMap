/**
 * Sidebar navigation, grouped. Paths are used rather than route names so a
 * module that has not been wired up yet degrades to the not-found view instead
 * of throwing during navigation.
 *
 * Labels are translation keys rather than text: this module is imported once at
 * load, so a literal would freeze the language chosen at that moment and never
 * follow the selector.
 */
export const NAV_GROUPS = [
  {
    labelKey: '',
    items: [
      { path: '/', labelKey: 'nav.dashboard', icon: 'home', exact: true }
    ]
  },
  {
    labelKey: 'nav.groups.money',
    items: [
      { path: '/accounts', labelKey: 'nav.accounts', icon: 'wallet' },
      { path: '/credit-cards', labelKey: 'nav.creditCards', icon: 'card' },
      { path: '/payments', labelKey: 'nav.payments', icon: 'receipt' }
    ]
  },
  {
    labelKey: 'nav.groups.spending',
    items: [
      { path: '/purchases', labelKey: 'nav.purchases', icon: 'bag' },
      { path: '/installments', labelKey: 'nav.installments', icon: 'layers' },
      { path: '/stores', labelKey: 'nav.stores', icon: 'store' }
    ]
  },
  {
    labelKey: 'nav.groups.planning',
    items: [
      { path: '/debts', labelKey: 'nav.debts', icon: 'debt' },
      { path: '/goals', labelKey: 'nav.goals', icon: 'target' },
      { path: '/job', labelKey: 'nav.job', icon: 'briefcase' }
    ]
  },
  {
    labelKey: 'nav.groups.insight',
    items: [
      { path: '/reports', labelKey: 'nav.reports', icon: 'report' },
      { path: '/notifications', labelKey: 'nav.notifications', icon: 'bell' }
    ]
  },
  {
    labelKey: '',
    items: [
      { path: '/settings', labelKey: 'nav.settings', icon: 'settings' }
    ]
  }
]
