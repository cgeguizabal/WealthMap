/**
 * Sidebar navigation, grouped. Paths are used rather than route names so a
 * module that has not been wired up yet degrades to the not-found view instead
 * of throwing during navigation.
 */
export const NAV_GROUPS = [
  {
    label: '',
    items: [
      { path: '/', label: 'Dashboard', icon: 'home', exact: true }
    ]
  },
  {
    label: 'Money',
    items: [
      { path: '/accounts', label: 'Accounts', icon: 'wallet' },
      { path: '/credit-cards', label: 'Credit cards', icon: 'card' },
      { path: '/payments', label: 'Payments', icon: 'receipt' }
    ]
  },
  {
    label: 'Spending',
    items: [
      { path: '/purchases', label: 'Purchases', icon: 'bag' },
      { path: '/installments', label: 'Installments', icon: 'layers' },
      { path: '/stores', label: 'Stores', icon: 'store' }
    ]
  },
  {
    label: 'Planning',
    items: [
      { path: '/debts', label: 'Debts', icon: 'debt' },
      { path: '/goals', label: 'Goals', icon: 'target' },
      { path: '/job', label: 'Job & income', icon: 'briefcase' }
    ]
  },
  {
    label: 'Insight',
    items: [
      { path: '/reports', label: 'Reports', icon: 'report' },
      { path: '/notifications', label: 'Notifications', icon: 'bell' }
    ]
  }
]
