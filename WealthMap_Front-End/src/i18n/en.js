/**
 * English copy. This file is the reference: every key here must exist in es.js,
 * and the dev-only check in index.js reports any that drift apart.
 *
 * Keys are grouped by where they appear, not by the words themselves, so a
 * screen's strings stay together when it is edited.
 */
export default {
  tour: {
    skip: 'Skip',
    back: 'Back',
    next: 'Next',
    done: 'Got it',
    replay: 'Show the tours again',
    replayHint: 'Plays the short walkthrough on each screen the next time you open it.',
    replayed: 'Tours reset. Open any screen to see it again.',

    dashboard: {
      welcome: {
        title: 'Welcome to WealthMap',
        body: 'A short tour on each screen, once. Skip any of them — you can replay them all from Settings.'
      },
      stats: {
        title: 'Where you stand',
        body: 'Available cash, credit still open, everything you owe, and what is safe to spend. All computed from what you have entered, never guessed.'
      },
      safeToSpend: {
        title: 'Safe to spend',
        body: 'What you can spend across accounts and cards and still pay every cutoff and due date on time. Salary you have not been paid yet counts, because a card lets you spend against it.'
      },
      alerts: {
        title: 'What needs attention',
        body: 'Cutoffs, due dates and goals that have fallen behind. Anything here has a date attached and will not wait.'
      }
    },

    accounts: {
      moving: {
        title: 'Deposit and withdraw',
        body: 'Every account card has Deposit and Withdraw. Use Deposit when money arrives outside your salary, and Withdraw when it leaves without being a purchase — a fee, an ATM, a transfer out of the app.'
      },
      transfer: {
        title: 'Move money between accounts',
        body: 'Transfer takes an amount out of one account and puts it into another in one step, recording a movement on both. It needs two accounts in the same currency, so it stays disabled until you have them.'
      },
      intro: {
        title: 'Accounts',
        body: 'Every place your money actually sits. Balances here are the ones the rest of the app reasons about.'
      },
      add: {
        title: 'Add an account',
        body: 'Name it, pick the bank and currency, and set the balance you can see in your bank right now. You can record the account number and a linked debit card too.'
      },
      list: {
        title: 'Your accounts',
        body: 'Deposit, withdraw or transfer from any card. Open one to see every movement with the balance recorded after it.'
      }
    },

    'credit-cards': {
      pay: {
        title: 'Pay a card',
        body: 'Pay opens the payment form. Choose the amount — Pay all fills in the closing statement — and say where the money came from: an account, which lowers that balance, or elsewhere, which does not. Paying frees the credit back up.'
      },
      detail: {
        title: 'Open a card for the detail',
        body: 'Click a card to see its purchases, its installment plans, and how much each plan is adding to the current statement. That last figure is the one that explains a bill you were not expecting.'
      },
      intro: {
        title: 'Credit cards',
        body: 'A card is not just a balance. What matters is how much falls in this statement, and when it must be paid.'
      },
      add: {
        title: 'Add a card',
        body: 'The cutoff day and payment due day are what let WealthMap tell you what is owed now versus next month. Get those two right and everything else follows.'
      },
      cutoff: {
        title: 'This statement vs next',
        body: 'Each card shows what falls in the closing statement separately from what rolls into the next one. Owing 100 does not mean paying 100 this month.'
      }
    },

    purchases: {
      method: {
        title: 'How you paid matters',
        body: 'Account lowers that account\'s balance. Credit card raises what the card owes and reduces its available credit. Cash affects neither — it is recorded so your spending totals are complete, not to move a balance.'
      },
      fixing: {
        title: 'Editing and deleting',
        body: 'Use the row actions to edit or delete. Deleting reverses everything the purchase did — the balance, the movement and the credit — so a wrong entry leaves nothing behind. Editing re-applies the difference the same way.'
      },
      intro: {
        title: 'Purchases',
        body: 'Record what you spend. Each purchase moves the account or card it was paid with, so balances stay true without editing them by hand.'
      },
      add: {
        title: 'Record a purchase',
        body: 'Pick how you paid — account, card or cash. Paying by card raises what that card owes; paying from an account lowers its balance.'
      },
      list: {
        title: 'Fixing mistakes',
        body: 'Edit or delete any purchase. A deletion reverses everything it did: the balance, the movement, and the credit it used.'
      }
    },

    installments: {
      create: {
        title: 'Creating a plan',
        body: 'Start a plan from Purchases: record the purchase on a credit card and choose to split it. Give the total price and the number of months, and WealthMap generates the schedule and charges the card the full amount up front.'
      },
      intro: {
        title: 'Installment plans',
        body: 'Interest-free purchases split over months. The card is charged the full price up front, and the plan repays it.'
      },
      list: {
        title: 'What is left',
        body: 'Each plan shows the payments remaining and which card it belongs to. Those payments are already counted in what is safe to spend.'
      }
    },

    debts: {
      paying: {
        title: 'Recording a payment',
        body: 'Open a debt and record a payment against it. Say whether the money left one of your accounts or came from outside; only the first moves a balance. The remaining amount drops, and next month\'s due figure follows.'
      },
      intro: {
        title: 'Debts',
        body: 'Loans and anything else you are paying down over time, separate from credit cards.'
      },
      add: {
        title: 'Add a debt',
        body: 'Record the original amount, what is left and the monthly due day. The monthly payment is then treated as committed money.'
      }
    },

    goals: {
      kinds: {
        title: 'Two kinds of goal',
        body: 'Savings goals are an amount by a date — an emergency fund, a trip. Product goals are a specific thing you are saving up for, with its price as the target. The tabs switch between them.'
      },
      contribute: {
        title: 'Adding to a goal',
        body: 'Open a goal and contribute to it. You can link a savings goal to an account, which lets WealthMap treat that money as spoken for rather than spendable.'
      },
      intro: {
        title: 'Goals',
        body: 'Two kinds: saving an amount by a date, and saving up for a specific thing.'
      },
      add: {
        title: 'Set a goal',
        body: 'Give it a target and a deadline, and WealthMap works out what you need to put aside each month to arrive on time.'
      }
    },

    job: {
      deductions: {
        title: 'Adding a deduction',
        body: 'Press Add on this card for every item your payslip subtracts — tax, social security, a pension, a loan repayment. Give it a name and a value, and WealthMap recalculates your net immediately.'
      },
      deductionKinds: {
        title: 'Fixed or percentage',
        body: 'A fixed deduction is the same amount every month. A percentage is worked out from your gross, so a raise updates it on its own. Pick percentage whenever your payslip shows a rate — entering today\'s figure as fixed makes it wrong the moment your salary changes.'
      },
      paydays: {
        title: 'When you are paid',
        body: 'Add up to three days of the month. Two entries means fortnightly, and each payday deposits its share automatically. A day past the end of a short month clamps to the last one, so the 31st pays on 28 February.'
      },
      extraIncome: {
        title: 'Income beyond salary',
        body: 'Add rent received, freelance work, or anything else arriving on a schedule. Recurring income counts toward what is safe to spend; one-off money is better recorded as a deposit on the account.'
      },
      intro: {
        title: 'Income',
        body: 'Your salary and anything else that arrives regularly. This is what makes future money real to the projections.'
      },
      salary: {
        title: 'Gross, deductions, net',
        body: 'Enter your gross salary and its deductions rather than the net figure. Storing the parts means the net is always right, even when a deduction changes.'
      }
    },

    reports: {
      download: {
        title: 'Download as PDF',
        body: 'View a month first, then download it. The PDF is written in the language the app is set to right now — switch to Spanish before downloading and the report comes out in Spanish.'
      },
      intro: {
        title: 'Monthly report',
        body: 'A full picture of one month: income, spending by category, every account and card, and how your goals moved.'
      },
      month: {
        title: 'Pick a month',
        body: 'Choose any month and view it, or download it as a PDF. The PDF is generated in whichever language you are using now.'
      }
    },

    settings: {
      appearance: {
        title: 'Appearance and language',
        body: 'Both live in the menu behind your initials, top right. Appearance offers light, dark, or following your system. Language switches the whole app, including the monthly report PDF.'
      },
      replay: {
        title: 'Replaying these tours',
        body: 'Show the tours again resets every one of them, so each screen walks you through it once more the next time you open it. Useful after a break, or when showing someone else around.'
      },
      intro: {
        title: 'Settings',
        body: 'Appearance, language and the defaults the app falls back on.'
      },
      bankDefaults: {
        title: 'Bank defaults',
        body: 'Which account to assume when a bank names none. One per bank, per direction — money in and money out can differ.'
      }
    }
  },

  theme: {
    label: 'Appearance',
    light: 'Light',
    dark: 'Dark',
    system: 'Match system'
  },

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
    settings: 'Settings',
    expand: 'Expand navigation',
    collapse: 'Collapse navigation',
    main: 'Main',
    openMenu: 'Open navigation',
    closeMenu: 'Close navigation',
    skipToContent: 'Skip to content'
  },

  common: {
    add: 'Add',
    save: 'Save',
    cancel: 'Cancel',
    confirm: 'Confirm',
    close: 'Close',
    closeDialog: 'Close dialog',
    dismiss: 'Dismiss',
    edit: 'Edit',
    remove: 'Remove',
    delete: 'Delete',
    add: 'Add',
    create: 'Create',
    continueLabel: 'Continue',
    keepIt: 'Keep it',
    tryAgain: 'Try again',
    saveChanges: 'Save changes',
    new: 'New',
    refresh: 'Refresh',
    loading: 'Loading…',
    saving: 'Saving…',
    search: 'Search',
    filter: 'Filter',
    apply: 'Apply',
    clear: 'Clear',
    all: 'All',
    none: 'None',
    optional: 'Optional',
    notes: 'Notes',
    name: 'Name',
    amount: 'Amount',
    date: 'Date',
    dateAndTime: 'Date and time',
    category: 'Category',
    description: 'Description',
    type: 'Type',
    kind: 'Kind',
    status: 'Status',
    currency: 'Currency',
    total: 'Total',
    completed: 'Completed',
    active: 'Active',
    monthly: 'Monthly',
    remaining: 'Remaining',
    paid: 'Paid',
    location: 'Location',
    overdue: 'Overdue',
    today: 'Today',
    tomorrow: 'Tomorrow',
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
    previousPage: 'Previous page',
    nextPage: 'Next page',
    pagination: 'Pagination',
    noResults: 'Nothing to show',
    notFoundHint: 'It may have been removed, or it is not yours.',
    goToDashboard: 'Go to dashboard',
    notFoundPage: "That page doesn't exist.",
    account: 'Account',
    chooseCategory: 'Choose a category',
    paidFrom: 'Paid from',
    paymentSource: 'Payment source',
    myAccounts: 'One of my accounts',
    myAccountsNote: 'Withdraws and records a movement',
    external: 'External',
    externalNote: 'Cash or someone else paid',
    noAccountsInCurrency: 'No {currency} accounts available',
    screenFailedTitle: 'This screen ran into a problem',
    screenFailedMessage: 'The rest of the app is still fine. Try again, or move to another section.'
  },

  auth: {
    loginTitle: 'Sign in',
    loginSubtitle: 'Pick up where your money left off.',
    registerTitle: 'Create your account',
    registerSubtitle: 'Takes a minute. Your data stays yours.',
    brandTagline: 'Your money, mapped. Figures are computed, never guessed.',
    email: 'Email',
    emailPlaceholder: 'you@example.com',
    password: 'Password',
    passwordPlaceholder: 'At least 8 characters',
    passwordHint: 'Minimum 8 characters.',
    fullName: 'Full name',
    fullNamePlaceholder: 'Ada Lovelace',
    country: 'Country',
    countryPlaceholder: 'El Salvador',
    currencyHint: 'Every total in WealthMap is shown in this currency.',
    signIn: 'Sign in',
    signUp: 'Create account',
    createOne: 'Create one',
    noAccount: 'No account yet?',
    haveAccount: 'Already have an account?',
    acceptTerms: 'I accept the {terms} and the {privacy}.',
    acceptTermsRequired: 'You must accept the Terms of Service and Privacy Policy.'
  },

  legal: {
    privacy: 'Privacy Policy',
    terms: 'Terms of Service',
    backToApp: '← Back to WealthMap',
    betaNotice: 'WealthMap is in beta. This document describes what the software does today and may change as it does.'
  },

  dashboard: {
    title: 'Dashboard',
    greeting: 'Good to see you, {name}',
    loadFailed: 'Could not load your dashboard',
    available: 'Available',
    availableCredit: 'Available credit',
    totalDebt: 'Total debt',
    safeToSpend: 'Safe to spend',
    safeToSpendHint: 'What you can spend across accounts and cards and still pay on time',
    netWorth: 'Net worth',
    netWorthNote: 'available minus everything owed',
    needsAttention: 'Needs attention',
    thisMonth: 'This month',
    spendableCash: 'Spendable cash',
    incomingSalary: 'Salary arriving',
    fallingDue: 'Falling due',
    spentSoFar: 'Spent so far',
    spendingAboveIncome: 'Spending above income',
    debtRatio: 'Debt ratio',
    upcoming: 'Upcoming',
    next30Days: 'Next 30 days',
    nothingDue: 'Nothing due',
    nothingDueMessage: 'No card, debt or installment payments in the next 30 days.',
    totalsCover: 'Totals cover',
    goalsTitle: 'Goals',
    noGoalsTitle: 'No goals yet',
    noGoalsMessage:
      'Set a target and WealthMap works out what you need to put aside each month.',
    createGoal: 'Create a goal',
    behind: 'Behind',
    viewAllGoals: 'View all goals'
  },

  accounts: {
    title: 'Accounts',
    subtitle: 'Every balance you hold, and where it sits.',
    newAccount: 'New account',
    editAccount: 'Edit account',
    transfer: 'Transfer',
    transferTitle: 'Transfer between accounts',
    totalHeld: 'Total held',
    deposit: 'Deposit',
    bonus: 'Bonus',
    withdraw: 'Withdraw',
    blocked: 'Blocked for saving',
    block: 'Block for saving',
    unblock: 'Unblock',
    blockAria: 'Block account for saving',
    unblockAria: 'Unblock account',
    deleteAria: 'Delete account',
    unblockedToast: '{name} unblocked.',
    blockedToast: '{name} blocked — deposits still work, withdrawals do not.',
    accountUnblocked: 'Account unblocked.',
    accountBlocked: 'Account blocked for saving.',
    bankName: 'Bank',
    accountNumber: 'Account number',
    debitCard: 'Debit card',
    debitCardHint: 'Does a debit card reach this account?',
    noDebitCard: 'None',
    physicalCard: 'Physical',
    digitalCard: 'Digital',
    debitCardLastFour: 'Debit card last 4',
    debitCardLastFourHint: 'The 4 digits on the card itself — a different number from the account.',
    numberUnknown: 'number not set',
    updatedToast: 'Account updated.',
    createdToast: '{name} is ready.',
    bankPlaceholder: 'BBVA',
    accountName: 'Account name',
    accountNamePlaceholder: 'Everyday checking',
    typeHint: 'Savings accounts can be blocked.',
    openingBalance: 'Opening balance',
    openingBalanceHint:
      'What is in the account today. Cannot be changed later — use deposits.',
    checking: 'Checking',
    savings: 'Savings',
    balance: 'Balance',
    currentBalance: 'Current balance',
    movements: 'Movements',
    movementsSubtitle: 'Newest first — every balance change is recorded',
    allAccounts: 'All accounts',
    backToAccounts: 'Back to accounts',
    notFound: 'Account not found',
    noAccountsTitle: 'No accounts yet',
    noAccountsMessage:
      'Money always lives in an account. Add your first one to start tracking.',
    addFirst: 'Add an account',
    createAccount: 'Create account',
    loadFailed: 'Could not load your accounts',
    needTwoToTransfer: 'You need two accounts to transfer',
    unblockToWithdraw: 'Unblock the account to withdraw',
    noMovementsTitle: 'No movements yet',
    noMovementsMessage: 'Deposits, withdrawals, transfers and payments all appear here.',
    depositKindHint: 'Salary and transfers are recorded automatically.',
    depositPlaceholder: 'Cash deposit',
    withdrawPlaceholder: 'Groceries',
    locationPlaceholder: 'Optional — ATM Reforma 222',
    withdrawHint: 'Withdrawals are recorded as ATM withdrawals; cash then leaves tracking.',
    from: 'From',
    to: 'To',
    chooseAccount: 'Choose an account',
    pickSourceFirst: 'Pick a source first',
    sameCurrencyHint: 'Only {currency} accounts — there is no conversion.',
    transferred: '{amount} moved to {name}.',
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
    newCard: 'New credit card',
    editCard: 'Edit card',
    deleteAria: 'Delete card',
    pay: 'Pay',
    limit: 'Limit',
    newLimit: 'New limit',
    updateLimit: 'Update credit limit',
    limitHint: 'Cannot be set below what is currently owed.',
    currentlyOwed: 'Currently owed',
    limitUpdated: 'Limit is now {amount}.',
    available: 'Available',
    availableCredit: 'Available credit',
    owed: 'Owed',
    owedOf: '{used} owed of {limit}',
    cardName: 'Card name',
    cardNamePlaceholder: 'Gold',
    creditLimit: 'Credit limit',
    interestRate: 'Annual interest rate',
    interest: 'Interest',
    paymentDueDay: 'Payment due day',
    clampsHint: 'Clamps in short months.',
    statementCutoff: 'Statement cutoff',
    dueDay: 'Due day',
    dueDayWithNumber: 'Due day {day}',
    statementCloses: 'Statement closes',
    installments: 'Installments',
    addsToStatement: 'Adds to statement',
    plansAddToStatement: 'Plans add to the statement due {date}',
    noPlansTitle: 'No installment plans on this card',
    noPlansMessage: 'Plans bought on this card will appear here with what each adds to the statement.',
    dueThisStatement: 'Due this statement',
    paidToast: '{paid} paid — {owed} still owed.',
    limitToast: 'Limit is now {limit}.',
    nextStatement: 'Next statement',
    futureInstallments: 'Future installments',
    paymentDue: 'Payment due',
    charges: 'Charges',
    charged: 'Charged',
    payments: 'Payments',
    registerPayment: 'Register payment',
    registerPaymentTitle: 'Register a card payment',
    paymentAmountHint: 'Cannot exceed what is owed.',
    payAll: 'Pay all',
    paymentMade: '{paid} paid — {owed} still owed.',
    notFound: 'Card not found',
    noChargesTitle: 'Nothing charged to this card',
    noChargesMessage:
      'Purchases paid with this card, and installment plans on it, appear here.',
    noPaymentsMessage:
      'Once you register a payment it appears here — including cash payments, which touch no account.',
    allCards: 'All cards',
    backToCards: 'Back to cards',
    noCardsTitle: 'No credit cards yet',
    noCardsMessage: 'Add a card to track its balance, available credit and due date.',
    addFirst: 'Add a card',
    addCard: 'Add card',
    limitInUse: '{percent}% of your limit in use',
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
    newPurchase: 'Record purchase',
    recordTitle: 'Record a purchase',
    item: 'Item',
    kindPurchase: 'Purchase',
    editTitle: 'Correct purchase',
    recordedToast: '{name} recorded.',
    updatedToast: '{name} corrected.',
    deletedToast: '{name} removed.',
    editAria: 'Correct {name}',
    deleteAria: 'Remove {name}',
    deleteTitle: 'Remove this purchase?',
    deleteMessage: '{name} for {amount} will be removed, and the money it moved put back.',
    deleteSecond: 'The purchase and its movement are deleted for good — this leaves no record that it existed.',
    store: 'Store',
    method: 'Method',
    paymentMethod: 'Payment method',
    debit: 'Debit',
    creditCard: 'Credit card',
    cash: 'Cash',
    noStore: 'No store',
    productName: 'What did you buy?',
    productPlaceholder: 'Groceries',
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
    thisPage: 'This page',
    paidWith: 'Paid with',
    debitNote: 'Withdraws from an account',
    creditNote: 'Charges a card',
    cashNote: 'Records only — cash is untracked',
    card: 'Card',
    chooseCard: 'Choose a card',
    noCards: 'No cards yet',
    chooseAccount: 'Choose an account',
    noAccounts: 'No accounts yet',
    cashCurrencyHint: 'Cash has no account to inherit a currency from.'
  },

  payments: {
    title: 'Payments',
    subtitle:
      'Everything you have paid against cards, debts and installment plans — whatever the money came from.',
    paid: 'Paid',
    source: 'Source',
    from: 'From',
    allTypes: 'All types',
    fromAccount: 'From account',
    cashOrThirdParty: 'Cash / third party',
    emptyTitle: 'No payments recorded',
    emptyMessage: 'Payments from any source appear here, including cash.'
  },

  installments: {
    title: 'Installments',
    subtitle:
      'Interest-free plans. The full price is charged to the card up front, then repaid month by month.',
    newPlan: 'New plan',
    createPlan: 'Create plan',
    planTitle: 'New installment plan',
    productLabel: 'What are you buying?',
    productPlaceholder: 'TV',
    card: 'Card',
    chooseCard: 'Choose a card',
    noCards: 'No cards yet',
    cardHint: 'The full price is charged to this card straight away.',
    totalPrice: 'Total price',
    months: 'Months',
    purchaseDate: 'Purchase date',
    interestFreeHint:
      'Interest free. The last payment carries the rounding so the plan sums exactly.',
    planCreated: '{name} split into {count} payments.',
    remaining: 'Remaining',
    stillToPay: 'Still to pay',
    paidOff: 'Paid off',
    paidOf: '{paid} of {total}',
    fullyPaid: 'Fully paid',
    payNext: 'Pay next',
    payNextTitle: 'Pay next installment',
    payInstallment: 'Pay installment',
    schedule: 'Schedule',
    scheduleSubtitle: 'Generated when the plan was created',
    scheduled: 'Scheduled',
    allPlans: 'All plans',
    backToPlans: 'Back to plans',
    notFound: 'Plan not found',
    paymentsLeft: '{remaining} of {total} payments left · last one {date}',
    lastPayment: 'Last payment {date}',
    progress: 'Progress',
    chargedTo: 'Charged to',
    cardRemoved: 'Card no longer available',
    paidToast: 'Installment paid — {remaining} payments left.',
    fullyPaidToast: '{name} is fully paid.',
    splitToast: '{name} split into {count} payments.',
    planKind: 'Installment plan',
    loadFailed: 'Could not load your plans',
    emptyTitle: 'No installment plans',
    emptyMessage:
      'Split a purchase across months at no interest. The card is charged in full today.',
    nothingOutstanding: 'Nothing outstanding',
    nothingCompleted: 'Nothing completed yet',
    allPaidOff: 'Every plan is paid off.',
    completedHint: 'Plans appear here once the last installment is paid.',
    addFirst: 'Create a plan',
    planSubtitle: '{count} interest-free payments · purchased {date}'
  },

  stores: {
    title: 'Stores',
    subtitle:
      'A shared catalogue — everyone sees every store, but only you can edit the ones you added.',
    newStore: 'New store',
    editStore: 'Edit store',
    storeName: 'Store name',
    logoUrl: 'Logo URL',
    addedByYou: 'Added by you',
    searchPlaceholder: 'Search by name or category',
    emptyTitle: 'No stores yet',
    emptyMessage: 'Add the shops you buy from so purchases can point at them.',
    addFirst: 'Add a store',
    addStore: 'Add store',
    noMatches: 'No matches',
    noMatchesFor: 'Nothing in the catalogue matches “{term}”.',
    namePlaceholder: 'Walmart',
    logoPlaceholder: 'https://…',
    logoHint: 'Optional. Must be a full URL.',
    noStore: 'No store',
    loadFailed: 'Could not load the catalogue'
  },

  debts: {
    title: 'Debts',
    subtitle: 'Loans and anything else you owe outside a credit card.',
    newDebt: 'New debt',
    editDebt: 'Edit debt',
    deleteAria: 'Delete debt',
    namePlaceholder: 'Car loan',
    originalAmount: 'Original amount',
    original: 'Original',
    stillOwed: 'Still owed',
    sameAsOriginal: 'Same as original',
    stillOwedHint: 'Only if you have already paid some of it down.',
    monthlyPayment: 'Monthly payment',
    dueDay: 'Due day',
    clampsHint: 'Clamps in short months.',
    remaining: 'Remaining',
    monthly: 'Monthly',
    nothingFurtherDue: 'Nothing further due',
    repaidOf: '{paid} of {total} repaid',
    markDefaulted: 'Mark as defaulted',
    defaultLabel: 'Default',
    markDefaultedTitle: 'Mark {name} as defaulted?',
    markDefaultedMessage:
      'It stays in your totals. Registering a payment later returns it to active.',
    registerPayment: 'Register payment',
    registerPaymentTitle: 'Register a debt payment',
    paymentHint: 'Cannot exceed what is left.',
    payAll: 'Pay all',
    defaultedHint: 'Paying a defaulted debt returns it to active.',
    payments: 'Payments',
    paymentsSubtitle: 'From an account or externally',
    paymentsEmpty: 'Payments against this debt appear here, including those paid in cash.',
    allDebts: 'All debts',
    backToDebts: 'Back to debts',
    notFound: 'Debt not found',
    loadFailed: 'Could not load your debts',
    emptyTitle: 'No debts recorded',
    emptyMessage:
      'Track a loan to see it in your totals, your safe-to-spend and your upcoming due dates.',
    addFirst: 'Add a debt',
    addDebt: 'Add debt',
    deleteTitle: 'Delete {name}?',
    deleteMessage: 'This removes the debt and its history. It cannot be undone.',
    deleted: '{name} deleted.'
  },

  goals: {
    title: 'Goals',
    subtitle: 'What you are saving toward, and what it takes each month to get there.',
    newSavingsGoal: 'New savings goal',
    newProductGoal: 'New product goal',
    editGoal: 'Edit goal',
    deleteAria: 'Delete goal',
    savings: 'Savings',
    products: 'Products',
    savingsPlaceholder: 'Emergency fund',
    productPlaceholder: 'PlayStation 6',
    target: 'Target',
    alreadySaved: 'Already saved',
    alreadySavedHint: 'Optional — what you have put aside for this already.',
    deadline: 'Deadline',
    noneSet: 'None set',
    savingsDeadlineHint: 'Drives the monthly figure and whether you are on track.',
    productDeadlineHint: 'Optional. Without one there is no required monthly amount.',
    linkedAccount: 'Linked savings account',
    trackOnly: 'None — track only',
    noSavingsAccount: 'No savings account in this currency',
    linkHint: 'Link one and contributing moves real money into it.',
    monthsLeft: 'Months left',
    neededMonthly: 'Needed monthly',
    contribute: 'Contribute',
    percentFunded: '{percent}% funded',
    ofTarget: 'of {amount}',
    toGo: '{amount} to go',
    fullyFundedToast: '{name} is fully funded.',
    addedToast: 'Added — {amount} saved so far.',
    createGoal: 'Create goal',
    linkedHint: 'Linked — contributing moves real money into the savings account',
    trackedOnlyHint: 'Tracked only — contributing does not move money',
    contributeTitle: 'Add to goal',
    moveFrom: 'Move from',
    chooseAccount: 'Choose an account',
    noEligibleAccounts: 'No eligible accounts',
    fillIt: 'Fill it',
    productNoMoney: 'Product goals track progress only — no money moves.',
    noLinkedAccount: 'This goal has no linked account, so nothing moves between accounts.',
    contributeHint:
      "This is a real transfer into the goal's savings account, recorded on both sides.",
    targetReached: 'Target already reached',
    completed: 'Completed',
    offPace: 'Off pace',
    noSavingsGoals: 'No savings goals yet',
    noProductGoals: 'No product goals yet',
    savingsEmptyMessage:
      'Set a target and a deadline, and WealthMap works out what to put aside each month.',
    productEmptyMessage:
      'Saving for something specific? Track it here — a deadline is optional.',
    deleteTitle: 'Delete {name}?',
    deleteMessage:
      'The goal and its progress are removed. Money already in a linked account stays put.',
    deleted: '{name} deleted.'
  },

  job: {
    title: 'Job & income',
    subtitle: 'Your salary, what comes out of it, and anything else that arrives regularly.',
    addJob: 'Add your job',
    saveJob: 'Save job',
    editJob: 'Edit job',
    deleteJob: 'Delete job',
    jobTitle: 'Job title',
    jobTitlePlaceholder: 'Full-stack developer',
    employer: 'Employer',
    employerPlaceholder: 'Acme',
    grossSalary: 'Gross monthly salary',
    grossHint: 'Before deductions.',
    paidInto: 'Paid into',
    chooseAccount: 'Choose an account',
    noAccounts: 'No accounts yet',
    noAccountInCurrency: 'No account in this currency',
    first: 'First',
    second: 'Second',
    third: 'Third',
    none: 'None',
    paymentDays: 'Payment days',
    paymentDaysHint:
      "Between one and three days a month. A day past the month's end clamps to the last day.",
    deductionsAfterSave: 'Deductions are added after the job is saved.',
    grossMonthly: 'Gross monthly',
    deducted: 'Deducted',
    netMonthly: 'Net monthly',
    perDeposit: 'Per deposit (after deductions)',
    paidOnDay: 'Paid on day',
    next: 'Next',
    deductions: 'Deductions',
    deductionsSubtitle: 'Taken from your payslip — the app does the arithmetic, not the tax law',
    addDeduction: 'Add deduction',
    editDeduction: 'Edit deduction',
    deductionNamePlaceholder: 'Income tax',
    deductionNameHint:
      'Copy it from your payslip — WealthMap does the arithmetic, not the tax law.',
    percentage: 'Percentage',
    percentageOfGross: 'Percentage of gross',
    fixedAmount: 'Fixed amount',
    takesOff: 'Takes off',
    netBecomes: 'Net monthly becomes',
    deductionsExceed: 'Deductions would exceed your gross salary. This will be rejected.',
    perMonth: '{amount} a month',
    nextPayday: 'Next:',
    perPayday: '{amount} at each of {count} paydays',
    noDeductionsTitle: 'No deductions',
    noDeductionsMessage: 'Net equals gross until you add what comes out.',
    deductionRemoved: 'Deduction removed.',
    removeDeductionTitle: 'Remove {name}?',
    removeDeductionMessage: 'Your net salary goes back up by this amount.',
    otherIncome: 'Other income',
    otherIncomeSubtitle: 'Recurring extras — one-off money is a bonus deposit on an account',
    noOtherIncome: 'No other income',
    noOtherIncomeMessage: 'Freelance work, rent, anything that arrives on a schedule.',
    addIncome: 'Add income',
    addRecurringIncome: 'Add recurring income',
    editIncome: 'Edit income',
    incomePlaceholder: 'Freelance',
    frequency: 'Frequency',
    countsAs: 'Counts as',
    // Trailing half of a sentence wrapped around a bolded figure. Split only
    // because the emphasis has to sit on the amount; both languages happen to
    // put it in the same place. A language that did not would need one key.
    perMonthInTotals: 'per month in your totals.',
    incomeHint:
      'This describes expected income. Money only appears in an account when you record a deposit.',
    monthlyEquivalent: 'Monthly equivalent',
    incomeRemoved: 'Income removed.',
    removeIncomeTitle: 'Remove {name}?',
    jobDeleted: 'Job deleted.',
    deleteJobTitle: 'Delete {name}?',
    deleteJobMessage: 'The job and all its deductions are removed. Your accounts are untouched.',
    emptyTitle: 'No job recorded',
    emptyMessage:
      'Add your salary and its deductions, and WealthMap works out your real take-home and when it lands.'
  },

  reports: {
    title: 'Monthly report',
    subtitle: 'What came in, what went out, and where you ended up.',
    download: 'Download PDF',
    pdf: 'PDF',
    reportMonth: 'Report month',
    view: 'View',
    month: 'Month',
    spendingByCategory: 'Spending by category',
    largestExpenses: 'Largest expenses',
    largestExpensesSubtitle: 'The five biggest single purchases',
    income: 'Income',
    spending: 'Spending',
    netResult: 'Net result',
    nothingToShow: 'Nothing to show',
    totalSpent: 'Total spent',
    expectedNetSalary: 'Expected net salary',
    perMonth: 'per month',
    noIncomeTitle: 'No income recorded this month',
    noIncomeMessage: 'Transfers between your own accounts are not counted as income.',
    accountsTitle: 'Accounts',
    colAccount: 'Account',
    colOpening: 'Opening',
    colIn: 'In',
    colOut: 'Out',
    colClosing: 'Closing',
    colCard: 'Card',
    colCharged: 'Charged',
    colPaid: 'Paid',
    colOwed: 'Owed',
    colAvailable: 'Available',
    accountsSubtitle: 'Opening to closing, movement by movement',
    noAccountsTitle: 'No accounts in this period',
    noAccountsMessage: 'Accounts opened after this month are not shown.',
    cardsTitle: 'Credit cards',
    cardsSubtitle: 'Balances are current, not month-end. Paid covers every source, cash included.',
    downloaded: 'Report downloaded.',
    downloadFailed: 'Could not generate the PDF. Try again.',
    loadFailed: 'Could not build the report'
  },

  notifications: {
    title: 'Notifications',
    subtitle: 'Alerts you have been shown. Checking again re-raises anything still true.',
    markAllRead: 'Mark all as read',
    markAsRead: 'Mark as read',
    checkNow: 'Check now',
    footnote: 'Marking something read is an acknowledgement, not a mute — if the condition still holds the next check raises it again.',
    unread: '{count} unread',
    unreadTab: 'Unread',
    readTab: 'Read',
    nothingUnread: 'Nothing unread',
    upToDate: 'You are up to date.',
    emptyTitle: 'No notifications yet',
    emptyMessage:
      'Use Check now to turn your current alerts into notifications you can work through.',
    markFailed: 'Could not mark that as read.',
    checkFailed: 'Could not check for new alerts.',
    nothingNew: 'Nothing new — everything current is already here.',
    created: '{count} new notifications.'
  },

/**
   * Values the API sends as-is: enum names and catalogue categories. Keyed by the
   * exact string the server returns, so a value that has not been mapped yet
   * falls through to the server's own wording rather than to a blank or a key.
   *
   * These are closed sets. Free prose from the server — domain error detail,
   * alert bodies — is handled in serverMessage, which is pattern-based.
   */
  server: {
    movementType: {
      SalaryDeposit: 'Salary deposit',
      Deposit: 'Deposit',
      Bonus: 'Bonus',
      TransferIn: 'Transfer in',
      TransferOut: 'Transfer out',
      Purchase: 'Purchase',
      Payment: 'Payment',
      AtmWithdrawal: 'ATM withdrawal'
    },
    paymentMethod: {
      DebitAccount: 'Debit',
      CreditCard: 'Credit card',
      Cash: 'Cash'
    },
    accountType: { Checking: 'Checking', Savings: 'Savings' },
    debitCardType: { None: 'No debit card', Physical: 'Debit card', Digital: 'Digital card' },
    paymentTarget: { CreditCard: 'Credit card', Debt: 'Debt', Installment: 'Installment' },
    paymentSource: { Account: 'From account', External: 'Cash / third party' },
    goalStatus: {
      OnTrack: 'On track',
      BehindSchedule: 'Behind schedule',
      DeadlinePassed: 'Deadline passed',
      Completed: 'Completed'
    },
    goalKind: { Savings: 'Savings', Product: 'Product' },
    debtStatus: { Active: 'Active', PaidOff: 'Paid off', Defaulted: 'Defaulted' },
    incomeFrequency: {
      Weekly: 'Weekly',
      Biweekly: 'Biweekly',
      Monthly: 'Monthly',
      Yearly: 'Yearly'
    },
    dueKind: { CreditCard: 'Credit card', Debt: 'Debt', Installment: 'Installment' },
    severity: { Info: 'Info', Warning: 'Warning', Critical: 'Critical' },
    deductionType: { Fixed: 'Fixed amount', Percentage: 'Percentage of gross' },
    category: {
      Food: 'Food',
      Groceries: 'Groceries',
      Restaurants: 'Restaurants',
      Transport: 'Transport',
      Electronics: 'Electronics',
      Clothing: 'Clothing',
      Health: 'Health',
      Entertainment: 'Entertainment',
      Home: 'Home',
      Services: 'Services',
      Education: 'Education',
      Travel: 'Travel',
      Other: 'Other'
    }
  },

  /** Error text from the server, matched by shape. See useServerText. */
  serverMessage: {
    validationFailed: 'Validation failed',
    businessRule: 'Business rule violation',
    notFound: 'Not found',
    unauthorized: 'You need to sign in again.',
    insufficientFunds: "Not enough money in '{name}'. Available {available}, requested {requested}.",
    exceedsCredit: "That exceeds the available credit on '{name}'.",
    alreadyArchived: 'That has already been deleted.',
    blockedAccount: "'{name}' is blocked for saving. Unblock it before taking money out.",
    futureDate: 'The date cannot be in the future.',
    currencyMismatch: 'The currencies do not match, and WealthMap does not convert between them.'
  },

/**
   * Alerts, keyed by the AlertType the API sends. Placeholders are filled from
   * the parts the server sends alongside the sentence, so the figures survive
   * translation. An alert whose type is missing here falls back to the English
   * the server composed.
   */
  alert: {
    CardPaymentDueSoon: {
      title: "'{name}' payment due in {daysUntil} day(s)",
      message: "You owe {amount} on '{name}', due {dueDate}."
    },
    DebtPaymentDueSoon: {
      title: "'{name}' payment due in {daysUntil} day(s)",
      message: "{amount} is due on {dueDate} for '{name}'."
    },
    InstallmentDueSoon: {
      title: "'{name}' payment due in {daysUntil} day(s)",
      message: "{amount} is due on {dueDate} for '{name}'."
    },
    InsufficientBalanceForCardPayment: {
      title: 'Checking balance will not cover upcoming card payments',
      message: '{owed} is due within {days} days but checking holds {checking}.',
      canCover: ' You could move {shortfall} from savings to cover it.',
      cannotCover: ' Savings would not cover the gap either.'
    },
    HighDebtRatio: {
      title: 'Debt payments take {ratio}% of your income',
      message:
        'Committed payments of {obligations} against a net income of {income}. ' +
        'Anything above {threshold}% leaves little room.'
    },
    OverspendingVsIncome: {
      title: 'Spending exceeds income this month',
      message: 'You have spent {spent} this month against a net income of {income}.'
    },
    GoalBehindSchedule: {
      title: "'{name}' is behind schedule",
      message: "'{name}' is {progress}% funded and trailing the pace needed to hit its deadline."
    },
    GoalDeadlinePassed: {
      title: "'{name}' missed its deadline",
      message: "'{name}' reached its deadline at {progress}% funded. Set a new deadline or adjust the target."
    },
    GoalReached: {
      title: "'{name}' is fully funded",
      message: "You have reached the target for '{name}'."
    }
  },

/** Sentences that wrap a figure. The number is a placeholder so word order can move. */
  composed: {
    dashboardNote: 'Everything below is computed from what you have recorded — nothing is estimated.',
    checkingAndSavings: '{checking} checking · {savings} savings',
    usedOfLimit: '{used} used of {limit} ({percent}%)',
    loansAndCards: '{loans} loans · {cards} cards',
    includesInstallments: 'includes {amount} in installment plans',
    spendableCashNote: 'excludes accounts blocked for saving',
    spendableOnCards: 'spendable now, settled at the due date',
    closesOn: 'closes {date}',
    closedOn: 'closed {date}',
    payBy: 'pay by {date}',
    notYetBilled: 'not on a statement yet',
    plusFutureInstallments: 'plus {amount} in future installments',
    installmentsSettled: { one: '{count} installment settled.', other: '{count} installments settled.' },
    planMeta: '{total} payments · {remaining} left',
    inDays: { one: 'in {count} day', other: 'in {count} days' },
    beforeDate: 'before {date}',
    cardsLoansInstallments: 'cards, loans and installments',
    safeUntil: 'safe until {date}',
    goalsOffPace: { one: '{count} goal off pace', other: '{count} goals off pace' },
    itemCount: { one: '{count} item', other: '{count} items' },
    andMore: 'and {count} more — see notifications',
    showingRecent: 'Showing the {shown} most recent of {total} purchases on this card.',
    nextDue: 'Next due {date}',
    dueOnDay: 'Due on day {day} each month',
    nextDate: ' · next {date}',
    newSavingsGoal: 'New savings goal',
    newProductGoal: 'New product goal',
    createSavingsGoal: 'Create a savings goal',
    createProductGoal: 'Create a product goal',
    installmentOf: 'Installment {number} of {total}',
    dueOn: 'Due {date}',
    perDepositTimes: 'Per deposit ({count}×)',
    reportPeriod: '{start} → {end} · all amounts in {currency}',
    reportDueDay: 'Due day {day}',
    generatedNote: 'Generated {when} · figures cover {currency} holdings only'
  },

  settings: {
    title: 'Settings',
    subtitle: 'How WealthMap treats your accounts and cards.'
  },

  tracking: {
    lastFour: 'Last 4 digits',
    lastFourHint:
      "The last 4 digits shown on your bank's transaction emails. Used for future automatic transaction sync.",
    mode: 'Tracking mode',
    manual: 'Manual',
    manualHint: "I'll enter transactions myself.",
    automatic: 'Automatic',
    automaticHint: 'Reserved for automatic bank email sync. Not yet available.',
    comingSoon: 'Coming soon'
  },

  bankDefaults: {
    title: 'Bank defaults',
    explain:
      "Some banks don't say which account a transfer came from. These defaults will be used when automatic sync is enabled.",
    add: 'Add default',
    newTitle: 'New bank default',
    editTitle: 'Edit bank default',
    bankName: 'Bank',
    accountNumber: 'Account number',
    debitCard: 'Debit card',
    debitCardHint: 'Does a debit card reach this account?',
    noDebitCard: 'None',
    physicalCard: 'Physical',
    digitalCard: 'Digital',
    debitCardLastFour: 'Debit card last 4',
    debitCardLastFourHint: 'The 4 digits on the card itself — a different number from the account.',
    numberUnknown: 'number not set',
    updatedToast: 'Account updated.',
    createdToast: '{name} is ready.',
    bankNamePlaceholder: 'Banco Agricola',
    direction: 'Direction',
    directionHint: 'Inbound and outbound are set separately.',
    inbound: 'Inbound',
    outbound: 'Outbound',
    account: 'Account',
    chooseAccount: 'Choose an account',
    saved: 'Bank default saved.',
    deleted: 'Bank default removed.',
    deleteTitle: 'Remove this default?',
    deleteMessage:
      'Transfers from {bank} will no longer have a fallback account when sync is enabled.',
    deleteSecond: 'This cannot be undone, but you can add it again at any time.',
    emptyTitle: 'No bank defaults yet',
    emptyMessage:
      'Nothing is wrong. Add one only if your bank sends transfer emails that never name the account.'
  },

  offline: {
    message: 'You are offline. Changes will fail until the connection is back.'
  }
}
