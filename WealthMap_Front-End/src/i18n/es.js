/**
 * Spanish copy. Latin American Spanish, using "tú" rather than "vos" or
 * "usted" — the app talks to one person about their own money.
 *
 * Keys mirror en.js exactly; the dev-only check in index.js reports drift.
 */
export default {
  language: {
    label: 'Idioma',
    english: 'English',
    spanish: 'Español'
  },

  nav: {
    groups: {
      money: 'Dinero',
      spending: 'Gastos',
      planning: 'Planificación',
      insight: 'Análisis'
    },
    dashboard: 'Panel',
    accounts: 'Cuentas',
    creditCards: 'Tarjetas de crédito',
    payments: 'Pagos',
    purchases: 'Compras',
    installments: 'Cuotas',
    stores: 'Comercios',
    debts: 'Deudas',
    goals: 'Metas',
    job: 'Trabajo e ingresos',
    reports: 'Informes',
    notifications: 'Notificaciones',
    expand: 'Expandir navegación',
    collapse: 'Contraer navegación',
    main: 'Principal',
    openMenu: 'Abrir navegación',
    closeMenu: 'Cerrar navegación'
  },

  common: {
    save: 'Guardar',
    cancel: 'Cancelar',
    confirm: 'Confirmar',
    close: 'Cerrar',
    edit: 'Editar',
    remove: 'Quitar',
    delete: 'Eliminar',
    add: 'Agregar',
    create: 'Crear',
    continueLabel: 'Continuar',
    keepIt: 'Conservar',
    tryAgain: 'Reintentar',
    loading: 'Cargando…',
    saving: 'Guardando…',
    search: 'Buscar',
    filter: 'Filtrar',
    all: 'Todo',
    none: 'Ninguno',
    optional: 'opcional',
    notes: 'Notas',
    name: 'Nombre',
    amount: 'Monto',
    date: 'Fecha',
    dateAndTime: 'Fecha y hora',
    category: 'Categoría',
    description: 'Descripción',
    type: 'Tipo',
    status: 'Estado',
    currency: 'Moneda',
    areYouSure: '¿Estás seguro?',
    proceedQuestion: '¿Seguro que quieres continuar?',
    somethingWentWrong: 'Algo salió mal.',
    totalsShownIn: 'Totales en {currency}',
    logout: 'Cerrar sesión',
    profile: 'Perfil',
    page: 'Página',
    of: 'de',
    previous: 'Anterior',
    next: 'Siguiente',
    noResults: 'Nada que mostrar'
  },

  auth: {
    loginTitle: 'Bienvenido de vuelta',
    loginSubtitle: 'Inicia sesión para continuar donde lo dejaste.',
    registerTitle: 'Crea tu cuenta',
    registerSubtitle: 'Lleva el control de lo que tienes, lo que debes y lo que viene.',
    email: 'Correo electrónico',
    password: 'Contraseña',
    fullName: 'Nombre completo',
    country: 'País',
    reportingCurrency: 'Moneda de referencia',
    signIn: 'Iniciar sesión',
    signUp: 'Crear cuenta',
    noAccount: '¿Aún no tienes cuenta?',
    haveAccount: '¿Ya tienes una cuenta?',
    signedOut: 'Se cerró tu sesión.'
  },

  dashboard: {
    title: 'Panel',
    subtitle: 'Cómo está tu dinero hoy.',
    totalAvailable: 'Total disponible',
    inChecking: 'En cuenta corriente',
    inSavings: 'En ahorro',
    availableCredit: 'Crédito disponible',
    totalDebt: 'Deuda total',
    netWorth: 'Patrimonio neto',
    safeToSpend: 'Puedes gastar',
    monthSpending: 'Gastado este mes',
    monthlyNetIncome: 'Ingreso neto mensual',
    monthlyObligations: 'Obligaciones mensuales',
    needsAttention: 'Requiere atención',
    upcoming: 'Próximos',
    nothingDue: 'Nada por vencer pronto.',
    allClear: 'Nada requiere tu atención.',
    excludedCurrencies: 'En otras monedas y fuera de estos totales: {list}'
  },

  accounts: {
    title: 'Cuentas',
    subtitle: 'Cada saldo que tienes, y dónde está.',
    newAccount: 'Nueva cuenta',
    transfer: 'Transferir',
    totalHeld: 'Total en cuentas',
    deposit: 'Depositar',
    withdraw: 'Retirar',
    blocked: 'Bloqueada para ahorro',
    block: 'Bloquear para ahorro',
    unblock: 'Desbloquear',
    unblockedToast: '{name} desbloqueada.',
    blockedToast: '{name} bloqueada — los depósitos siguen funcionando, los retiros no.',
    bankName: 'Banco',
    checking: 'Corriente',
    savings: 'Ahorro',
    balance: 'Saldo',
    movements: 'Movimientos',
    noAccountsTitle: 'Aún no hay cuentas',
    noAccountsMessage: 'El dinero siempre vive en una cuenta. Agrega la primera para empezar.',
    addFirst: 'Agregar una cuenta',
    loadFailed: 'No se pudieron cargar tus cuentas',
    needTwoToTransfer: 'Necesitas dos cuentas para transferir',
    unblockToWithdraw: 'Desbloquea la cuenta para retirar',
    noMovementsTitle: 'Aún no hay movimientos',
    noMovementsMessage: 'Depósitos, retiros, transferencias y pagos aparecen aquí.',
    deleteTitle: '¿Eliminar {name}?',
    deleteMessage:
      '{name} se quitará de tus cuentas, saldos y totales.{balance} Su historial de movimientos ' +
      'se conserva, y las compras y pagos hechos desde ella quedan registrados.',
    deleteBalanceNote: ' Todavía tiene {amount}.',
    deleteSecond:
      'Esto quita {name} de WealthMap. Ya no podrás depositar, retirar ni transferir con ella.',
    deleted: '{name} eliminada. Su historial se conservó.'
  },

  cards: {
    title: 'Tarjetas de crédito',
    subtitle: 'El crédito disponible es el límite menos lo que debes — siempre calculado.',
    newCard: 'Nueva tarjeta',
    pay: 'Pagar',
    limit: 'Límite',
    available: 'Disponible',
    owed: 'Adeudado',
    dueDay: 'Vence el día {day}',
    charges: 'Cargos',
    payments: 'Pagos',
    noCardsTitle: 'Aún no hay tarjetas',
    noCardsMessage: 'Agrega una tarjeta para seguir su saldo, crédito disponible y fecha de pago.',
    addFirst: 'Agregar una tarjeta',
    loadFailed: 'No se pudieron cargar tus tarjetas',
    nothingOwed: 'No debes nada en esta tarjeta',
    deleteTitle: '¿Eliminar {name}?',
    deleteMessage:
      '{name} se quitará de tus tarjetas y de tu crédito disponible.{owed} Sus compras, planes ' +
      'de cuotas y pagos se conservan.',
    deleteOwedNote: ' Todavía debe {amount}, y eliminarla no salda esa deuda.',
    deleteSecond:
      'Esto quita {name} de WealthMap. Ya no podrás cargarle compras ni registrar pagos.',
    deleted: '{name} eliminada. Su historial se conservó.'
  },

  purchases: {
    title: 'Compras',
    subtitle: 'Todo lo que has comprado, sin importar cómo lo pagaste.',
    newPurchase: 'Nueva compra',
    item: 'Artículo',
    store: 'Comercio',
    method: 'Método',
    debit: 'Débito',
    creditCard: 'Tarjeta de crédito',
    cash: 'Efectivo',
    noStore: 'Sin comercio',
    productName: '¿Qué compraste?',
    emptyTitle: 'No se encontraron compras',
    emptyMessage: 'Nada coincide con estos filtros — o todavía no has registrado nada.',
    saved: 'Compra guardada.',
    year: 'Año',
    month: 'Mes',
    allYears: 'Todos los años',
    allMonths: 'Todos los meses',
    allCategories: 'Todas las categorías',
    pickYearFirst: 'Elige un año primero',
    apply: 'Aplicar',
    clear: 'Limpiar',
    thisPage: 'Esta página'
  },

  payments: {
    title: 'Pagos',
    subtitle: 'Cada pago que has hecho, desde cualquier origen.',
    paid: 'Pagado',
    source: 'Origen',
    fromAccount: 'Desde cuenta',
    cashOrThirdParty: 'Efectivo / terceros',
    emptyTitle: 'No hay pagos registrados',
    emptyMessage: 'Los pagos de cualquier origen aparecen aquí, incluido el efectivo.'
  },

  installments: {
    title: 'Cuotas',
    subtitle: 'Lo que sigues pagando, mes a mes.',
    newPlan: 'Nuevo plan',
    remaining: 'Restante',
    paidOff: 'Pagado',
    installmentsLabel: 'Cuotas',
    emptyTitle: 'No hay planes de cuotas',
    emptyMessage: 'Una compra dividida en meses aparece aquí.'
  },

  stores: {
    title: 'Comercios',
    subtitle: 'Los lugares donde compras.',
    newStore: 'Nuevo comercio',
    emptyTitle: 'Aún no hay comercios',
    emptyMessage: 'Agrega los lugares donde compras para que cada compra diga dónde ocurrió.'
  },

  debts: {
    title: 'Deudas',
    subtitle: 'Lo que debes, y qué tan rápido está bajando.',
    newDebt: 'Nueva deuda',
    emptyTitle: 'No hay deudas',
    emptyMessage: 'Préstamos y cualquier otra cosa que debas van aquí.'
  },

  goals: {
    title: 'Metas',
    subtitle: 'Para qué estás ahorrando.',
    newGoal: 'Nueva meta',
    saved: 'Ahorrado',
    target: 'Objetivo',
    emptyTitle: 'Aún no hay metas',
    emptyMessage: 'Define algo para ahorrar y mira qué tan cerca estás.'
  },

  job: {
    title: 'Trabajo e ingresos',
    subtitle: 'Tu salario, sus deducciones, y lo que realmente llega.',
    grossMonthly: 'Bruto mensual',
    deducted: 'Deducido',
    netMonthly: 'Neto mensual',
    perDeposit: 'Por depósito (después de deducciones)',
    paidOnDay: 'Se paga el día',
    next: 'Próximos',
    deductions: 'Deducciones',
    deductionsSubtitle: 'Tomadas de tu planilla — la app hace la aritmética, no la ley fiscal',
    percentageOfGross: 'Porcentaje del bruto',
    fixedAmount: 'Monto fijo',
    perMonth: '{amount} al mes',
    perPayday: '{amount} en cada uno de {count} pagos',
    noDeductionsTitle: 'Sin deducciones',
    otherIncome: 'Otros ingresos',
    emptyTitle: 'Aún no hay trabajo',
    emptyMessage:
      'Agrega tu salario y sus deducciones, y WealthMap calcula lo que realmente recibes y cuándo llega.'
  },

  reports: {
    title: 'Informes',
    subtitle: 'Un mes a la vez, en tu moneda de referencia.',
    download: 'Descargar PDF',
    spendingByCategory: 'Gasto por categoría',
    largestExpenses: 'Gastos más grandes',
    largestExpensesSubtitle: 'Las cinco compras individuales más grandes',
    income: 'Ingresos',
    netResult: 'Resultado neto',
    nothingToShow: 'Nada que mostrar'
  },

  notifications: {
    title: 'Notificaciones',
    subtitle: 'Lo que WealthMap detectó.',
    markAllRead: 'Marcar todo como leído',
    unread: '{count} sin leer',
    emptyTitle: 'Nada por ahora',
    emptyMessage: 'Las alertas sobre vencimientos y saldos aparecerán aquí.'
  },

  offline: {
    message: 'Estás sin conexión. Los cambios fallarán hasta que vuelva.'
  }
}
