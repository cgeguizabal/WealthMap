/**
 * English copy. This file is the reference: every key here must exist in es.js,
 * and the dev-only check in index.js reports any that drift apart.
 *
 * Keys are grouped by where they appear, not by the words themselves, so a
 * screen's strings stay together when it is edited.
 */
export default {
  language: {
    label: 'Language',
    english: 'English',
    spanish: 'Español'
  },

  nav: {
    groups: {
      money: 'Money',
      spending: 'Spending',
      planning: 'Planning',
      insight: 'Insight'
    },
    dashboard: 'Dashboard',
    accounts: 'Accounts',
    creditCards: 'Credit cards',
    payments: 'Payments',
    purchases: 'Purchases',
    installments: 'Installments',
    stores: 'Stores',
    debts: 'Debts',
    goals: 'Goals',
    job: 'Job & income',
    reports: 'Reports',
    notifications: 'Notifications',
    expand: 'Expand navigation',
    collapse: 'Collapse navigation',
    main: 'Main',
    openMenu: 'Open navigation',
    closeMenu: 'Close navigation'
  },

  common: {
    save: 'Save',
    cancel: 'Cancel',
    confirm: 'Confirm',
    close: 'Close',
    edit: 'Edit',
    remove: 'Remove',
    delete: 'Delete',
    add: 'Add',
    create: 'Create',
    continueLabel: 'Continue',
    keepIt: 'Keep it',
    tryAgain: 'Try again',
    loading: 'Loading…',
    saving: 'Saving…',
    search: 'Search',
    filter: 'Filter',
    all: 'All',
    none: 'None',
    optional: 'optional',
    notes: 'Notes',
    name: 'Name',
    amount: 'Amount',
    date: 'Date',
    dateAndTime: 'Date and time',
    category: 'Category',
    description: 'Description',
    type: 'Type',
    status: 'Status',
    currency: 'Currency',
    areYouSure: 'Are you sure?',
    proceedQuestion: 'Are you sure you want to proceed?',
    somethingWentWrong: 'Something went wrong.',
    totalsShownIn: 'Totals shown in {currency}',
    logout: 'Sign out',
    profile: 'Profile',
    page: 'Page',
    of: 'of',
    previous: 'Previous',
    next: 'Next',
    noResults: 'Nothing to show'
  },

  auth: {
    loginTitle: 'Welcome back',
    loginSubtitle: 'Sign in to pick up where you left off.',
    registerTitle: 'Create your account',
    registerSubtitle: 'Track what you hold, what you owe and what is coming.',
    email: 'Email',
    password: 'Password',
    fullName: 'Full name',
    country: 'Country',
    reportingCurrency: 'Reporting currency',
    signIn: 'Sign in',
    signUp: 'Create account',
    noAccount: 'No account yet?',
    haveAccount: 'Already have an account?',
    signedOut: 'You have been signed out.'
  },

  dashboard: {
    title: 'Dashboard',
    subtitle: 'Where your money stands today.',
    totalAvailable: 'Total available',
    inChecking: 'In checking',
    inSavings: 'In savings',
    availableCredit: 'Available credit',
    totalDebt: 'Total debt',
    netWorth: 'Net worth',
    safeToSpend: 'Safe to spend',
    monthSpending: 'Spent this month',
    monthlyNetIncome: 'Monthly net income',
    monthlyObligations: 'Monthly obligations',
    needsAttention: 'Needs attention',
    upcoming: 'Upcoming',
    nothingDue: 'Nothing due soon.',
    allClear: 'Nothing needs your attention.',
    excludedCurrencies: 'Held in other currencies and left out of these totals: {list}'
  },

  accounts: {
    title: 'Accounts',
    subtitle: 'Every balance you hold, and where it sits.',
    newAccount: 'New account',
    transfer: 'Transfer',
    totalHeld: 'Total held',
    deposit: 'Deposit',
    withdraw: 'Withdraw',
    blocked: 'Blocked for saving',
    block: 'Block for saving',
    unblock: 'Unblock',
    unblockedToast: '{name} unblocked.',
    blockedToast: '{name} blocked — deposits still work, withdrawals do not.',
    bankName: 'Bank',
    checking: 'Checking',
    savings: 'Savings',
    balance: 'Balance',
    movements: 'Movements',
    noAccountsTitle: 'No accounts yet',
    noAccountsMessage: 'Money always lives in an account. Add your first one to start tracking.',
    addFirst: 'Add an account',
    loadFailed: 'Could not load your accounts',
    needTwoToTransfer: 'You need two accounts to transfer',
    unblockToWithdraw: 'Unblock the account to withdraw',
    noMovementsTitle: 'No movements yet',
    noMovementsMessage: 'Deposits, withdrawals, transfers and payments all appear here.',
    deleteTitle: 'Delete {name}?',
    deleteMessage:
      '{name} will be removed from your accounts, balances and totals.{balance} Its movement ' +
      'history is kept, and past purchases and payments made from it stay on record.',
    deleteBalanceNote: ' It still holds {amount}.',
    deleteSecond:
      'This removes {name} from WealthMap. You will not be able to deposit, withdraw or ' +
      'transfer with it again.',
    deleted: '{name} deleted. Its history was kept.'
  },

  cards: {
    title: 'Credit cards',
    subtitle: 'Available credit is limit minus what you owe — always computed.',
    newCard: 'New card',
    pay: 'Pay',
    limit: 'Limit',
    available: 'Available',
    owed: 'Owed',
    dueDay: 'Due day {day}',
    charges: 'Charges',
    payments: 'Payments',
    noCardsTitle: 'No credit cards yet',
    noCardsMessage: 'Add a card to track its balance, available credit and due date.',
    addFirst: 'Add a card',
    loadFailed: 'Could not load your cards',
    nothingOwed: 'Nothing owed on this card',
    deleteTitle: 'Delete {name}?',
    deleteMessage:
      '{name} will be removed from your cards and your available credit.{owed} Its purchases, ' +
      'installment plans and payments are kept.',
    deleteOwedNote: ' It still has {amount} owed, and deleting it will not pay that off.',
    deleteSecond:
      'This removes {name} from WealthMap. You will not be able to charge purchases to it or ' +
      'record payments against it again.',
    deleted: '{name} deleted. Its history was kept.'
  },

  purchases: {
    title: 'Purchases',
    subtitle: 'Everything you have bought, however you paid for it.',
    newPurchase: 'New purchase',
    item: 'Item',
    store: 'Store',
    method: 'Method',
    debit: 'Debit',
    creditCard: 'Credit card',
    cash: 'Cash',
    noStore: 'No store',
    productName: 'What did you buy?',
    emptyTitle: 'No purchases found',
    emptyMessage: 'Nothing matches these filters — or nothing has been recorded yet.',
    saved: 'Purchase saved.',
    year: 'Year',
    month: 'Month',
    allYears: 'All years',
    allMonths: 'All months',
    allCategories: 'All categories',
    pickYearFirst: 'Pick a year first',
    apply: 'Apply',
    clear: 'Clear',
    thisPage: 'This page'
  },

  payments: {
    title: 'Payments',
    subtitle: 'Every payment you have made, from any source.',
    paid: 'Paid',
    source: 'Source',
    fromAccount: 'From account',
    cashOrThirdParty: 'Cash / third party',
    emptyTitle: 'No payments recorded',
    emptyMessage: 'Payments from any source appear here, including cash.'
  },

  installments: {
    title: 'Installments',
    subtitle: 'What you are still paying off, month by month.',
    newPlan: 'New plan',
    remaining: 'Remaining',
    paidOff: 'Paid off',
    installmentsLabel: 'Installments',
    emptyTitle: 'No installment plans',
    emptyMessage: 'A purchase split over months shows up here.'
  },

  stores: {
    title: 'Stores',
    subtitle: 'The shops behind your purchases.',
    newStore: 'New store',
    emptyTitle: 'No stores yet',
    emptyMessage: 'Add the shops you buy from so purchases can name where they happened.'
  },

  debts: {
    title: 'Debts',
    subtitle: 'What you owe, and how fast it is coming down.',
    newDebt: 'New debt',
    emptyTitle: 'No debts',
    emptyMessage: 'Loans and anything else you owe belong here.'
  },

  goals: {
    title: 'Goals',
    subtitle: 'What you are saving toward.',
    newGoal: 'New goal',
    saved: 'Saved',
    target: 'Target',
    emptyTitle: 'No goals yet',
    emptyMessage: 'Set something to save toward and track how close you are.'
  },

  job: {
    title: 'Job & income',
    subtitle: 'Your salary, its deductions, and what actually lands.',
    grossMonthly: 'Gross monthly',
    deducted: 'Deducted',
    netMonthly: 'Net monthly',
    perDeposit: 'Per deposit (after deductions)',
    paidOnDay: 'Paid on day',
    next: 'Next',
    deductions: 'Deductions',
    deductionsSubtitle: 'Taken from your payslip — the app does the arithmetic, not the tax law',
    percentageOfGross: 'Percentage of gross',
    fixedAmount: 'Fixed amount',
    perMonth: '{amount} a month',
    perPayday: '{amount} at each of {count} paydays',
    noDeductionsTitle: 'No deductions',
    otherIncome: 'Other income',
    emptyTitle: 'No job yet',
    emptyMessage:
      'Add your salary and its deductions, and WealthMap works out your real take-home and when it lands.'
  },

  reports: {
    title: 'Reports',
    subtitle: 'A month at a time, in your reporting currency.',
    download: 'Download PDF',
    spendingByCategory: 'Spending by category',
    largestExpenses: 'Largest expenses',
    largestExpensesSubtitle: 'The five biggest single purchases',
    income: 'Income',
    netResult: 'Net result',
    nothingToShow: 'Nothing to show'
  },

  notifications: {
    title: 'Notifications',
    subtitle: 'What WealthMap noticed.',
    markAllRead: 'Mark all as read',
    unread: '{count} unread',
    emptyTitle: 'Nothing yet',
    emptyMessage: 'Alerts about due dates and balances will show up here.'
  },

  offline: {
    message: 'You are offline. Changes will fail until the connection is back.'
  }
}
