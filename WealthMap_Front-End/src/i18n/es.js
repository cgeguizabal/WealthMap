/**
 * Spanish copy. Latin American Spanish, using "tú" rather than "vos" or
 * "usted" — the app talks to one person about their own money.
 *
 * Keys mirror en.js exactly; the dev-only check in index.js reports drift.
 */
export default {
  tour: {
    skip: 'Omitir',
    back: 'Atrás',
    next: 'Siguiente',
    done: 'Entendido',
    replay: 'Volver a mostrar los recorridos',
    replayHint: 'Vuelve a reproducir la guía breve de cada pantalla la próxima vez que la abras.',
    replayed: 'Recorridos reiniciados. Abre cualquier pantalla para verlos otra vez.',

    dashboard: {
      welcome: {
        title: 'Bienvenido a WealthMap',
        body: 'Un recorrido breve en cada pantalla, una sola vez. Puedes omitir cualquiera: se reinician desde Ajustes.'
      },
      stats: {
        title: 'Cómo estás',
        body: 'Efectivo disponible, crédito libre, todo lo que debes y cuánto puedes gastar con seguridad. Todo calculado con lo que registraste, nunca estimado.'
      },
      safeToSpend: {
        title: 'Puedes gastar',
        body: 'Lo que puedes gastar entre cuentas y tarjetas sin dejar de pagar cada corte y cada fecha límite a tiempo. El salario que aún no recibes cuenta, porque una tarjeta te permite gastar contra él.'
      },
      alerts: {
        title: 'Requiere atención',
        body: 'Cortes, fechas de pago y metas que se quedaron atrás. Todo lo que aparece aquí tiene una fecha y no espera.'
      }
    },

    accounts: {
      moving: {
        title: 'Depositar y retirar',
        body: 'Cada tarjeta de cuenta tiene Depositar y Retirar. Usa Depositar cuando entre dinero que no sea tu salario, y Retirar cuando salga sin ser una compra: una comisión, un cajero, una transferencia fuera de la aplicación.'
      },
      transfer: {
        title: 'Mover dinero entre cuentas',
        body: 'Transferir saca un monto de una cuenta y lo pone en otra en un solo paso, dejando un movimiento en ambas. Necesita dos cuentas en la misma moneda, por eso está deshabilitado hasta que las tengas.'
      },
      intro: {
        title: 'Cuentas',
        body: 'Cada lugar donde realmente está tu dinero. Estos saldos son los que usa el resto de la aplicación.'
      },
      add: {
        title: 'Agregar una cuenta',
        body: 'Ponle nombre, elige el banco y la moneda, y registra el saldo que ves ahora en tu banco. También puedes anotar el número de cuenta y la tarjeta de débito vinculada.'
      },
      list: {
        title: 'Tus cuentas',
        body: 'Deposita, retira o transfiere desde cualquier tarjeta. Ábrela para ver cada movimiento con el saldo que quedó después.'
      }
    },

    'credit-cards': {
      pay: {
        title: 'Pagar una tarjeta',
        body: 'Pagar abre el formulario de pago. Elige el monto — Pagar todo llena el estado de cuenta que cierra — e indica de dónde salió el dinero: de una cuenta, lo que baja ese saldo, o de fuera, que no. Pagar libera el crédito otra vez.'
      },
      detail: {
        title: 'Abre una tarjeta para ver el detalle',
        body: 'Haz clic en una tarjeta para ver sus compras, sus planes de cuotas y cuánto está sumando cada plan al corte actual. Esa última cifra es la que explica una factura que no esperabas.'
      },
      intro: {
        title: 'Tarjetas de crédito',
        body: 'Una tarjeta no es solo un saldo. Lo que importa es cuánto cae en este estado de cuenta y cuándo hay que pagarlo.'
      },
      add: {
        title: 'Agregar una tarjeta',
        body: 'El día de corte y el día de pago son los que permiten a WealthMap decirte qué debes ahora y qué pasa al mes siguiente. Con esos dos bien, lo demás sale solo.'
      },
      cutoff: {
        title: 'Este corte y el siguiente',
        body: 'Cada tarjeta muestra por separado lo que cae en el corte que cierra y lo que pasa al siguiente. Deber 100 no significa pagar 100 este mes.'
      }
    },

    purchases: {
      method: {
        title: 'Cómo pagaste sí importa',
        body: 'Cuenta baja el saldo de esa cuenta. Tarjeta de crédito aumenta lo que la tarjeta debe y reduce su crédito disponible. Efectivo no afecta ninguno: se registra para que tus totales de gasto estén completos, no para mover un saldo.'
      },
      fixing: {
        title: 'Editar y eliminar',
        body: 'Usa las acciones de cada fila para editar o eliminar. Al eliminar se revierte todo lo que la compra hizo — el saldo, el movimiento y el crédito — así que un registro equivocado no deja rastro. Editar aplica la diferencia de la misma forma.'
      },
      intro: {
        title: 'Compras',
        body: 'Registra lo que gastas. Cada compra mueve la cuenta o la tarjeta con la que pagaste, así los saldos se mantienen correctos sin editarlos a mano.'
      },
      add: {
        title: 'Registrar una compra',
        body: 'Elige cómo pagaste: cuenta, tarjeta o efectivo. Pagar con tarjeta aumenta lo que esa tarjeta debe; pagar desde una cuenta baja su saldo.'
      },
      list: {
        title: 'Corregir errores',
        body: 'Edita o elimina cualquier compra. Al eliminarla se revierte todo lo que hizo: el saldo, el movimiento y el crédito que usó.'
      }
    },

    installments: {
      create: {
        title: 'Crear un plan',
        body: 'Los planes empiezan en Compras: registra la compra con una tarjeta de crédito y elige dividirla. Indica el precio total y el número de meses, y WealthMap genera el calendario y carga la tarjeta con el monto completo desde el inicio.'
      },
      intro: {
        title: 'Planes de cuotas',
        body: 'Compras sin intereses divididas en meses. La tarjeta se carga con el precio completo desde el inicio, y el plan lo va pagando.'
      },
      list: {
        title: 'Lo que falta',
        body: 'Cada plan muestra las cuotas que faltan y a qué tarjeta pertenece. Esas cuotas ya están contadas en lo que puedes gastar con seguridad.'
      }
    },

    debts: {
      paying: {
        title: 'Registrar un pago',
        body: 'Abre una deuda y registra un pago. Indica si el dinero salió de una de tus cuentas o vino de fuera; solo lo primero mueve un saldo. El monto restante baja, y la cuota del mes siguiente se ajusta.'
      },
      intro: {
        title: 'Deudas',
        body: 'Préstamos y todo lo demás que estás pagando poco a poco, aparte de las tarjetas de crédito.'
      },
      add: {
        title: 'Agregar una deuda',
        body: 'Registra el monto original, lo que falta y el día de pago mensual. La cuota pasa a contarse como dinero comprometido.'
      }
    },

    goals: {
      kinds: {
        title: 'Dos tipos de meta',
        body: 'Las metas de ahorro son una cantidad para una fecha: un fondo de emergencia, un viaje. Las metas de producto son algo concreto para lo que ahorras, con su precio como objetivo. Las pestañas cambian entre ellas.'
      },
      contribute: {
        title: 'Aportar a una meta',
        body: 'Abre una meta y aporta a ella. Puedes vincular una meta de ahorro a una cuenta, lo que permite a WealthMap tratar ese dinero como comprometido en lugar de disponible.'
      },
      intro: {
        title: 'Metas',
        body: 'Dos tipos: ahorrar una cantidad para una fecha, y ahorrar para algo concreto.'
      },
      add: {
        title: 'Crear una meta',
        body: 'Ponle un objetivo y una fecha límite, y WealthMap calcula cuánto necesitas apartar cada mes para llegar a tiempo.'
      }
    },

    job: {
      freelance: {
        title: 'Trabajo independiente',
        body: 'Trabajo sin calendario. Agrega lo que acordaste, márcalo como entregado al terminar, y registra el pago cuando el cliente realmente pague, que puede ser un mes después o nunca.'
      },
      freelancePaid: {
        title: 'El trabajo sin cobrar no es dinero',
        body: 'Lo que un cliente te debe se muestra aquí pero no cuenta para nada. Solo cuando pulsas Me pagaron el monto entra en una cuenta, y desde ese momento sube lo que puedes gastar con seguridad como cualquier otro saldo.'
      },
      deductions: {
        title: 'Agregar una deducción',
        body: 'Pulsa Agregar en esta tarjeta por cada concepto que tu boleta descuenta: impuestos, seguro social, una pensión, el pago de un préstamo. Ponle nombre y valor, y WealthMap recalcula tu neto al instante.'
      },
      deductionKinds: {
        title: 'Fija o porcentaje',
        body: 'Una deducción fija es el mismo monto cada mes. Un porcentaje se calcula sobre tu bruto, así que un aumento lo actualiza solo. Elige porcentaje siempre que tu boleta muestre una tasa: poner la cifra de hoy como fija la vuelve incorrecta en cuanto cambie tu salario.'
      },
      paydays: {
        title: 'Cuándo te pagan',
        body: 'Agrega hasta tres días del mes. Dos entradas significan quincenal, y cada día de pago deposita su parte automáticamente. Un día posterior al final de un mes corto se ajusta al último, así que el 31 se paga el 28 de febrero.'
      },
      extraIncome: {
        title: 'Ingresos además del salario',
        body: 'Agrega alquileres que recibas, trabajo independiente o cualquier otra cosa que llegue con regularidad. Los ingresos recurrentes cuentan para lo que puedes gastar con seguridad; el dinero puntual conviene registrarlo como un depósito en la cuenta.'
      },
      intro: {
        title: 'Ingresos',
        body: 'Tu salario y todo lo demás que llega con regularidad. Esto es lo que hace que el dinero futuro sea real para las proyecciones.'
      },
      salary: {
        title: 'Bruto, deducciones, neto',
        body: 'Registra tu salario bruto y sus deducciones en lugar del neto. Guardar las partes hace que el neto siempre sea correcto, incluso si cambia una deducción.'
      }
    },

    reports: {
      download: {
        title: 'Descargar en PDF',
        body: 'Primero mira un mes, luego descárgalo. El PDF se escribe en el idioma que la aplicación tenga ahora mismo: cambia a inglés antes de descargar y el reporte sale en inglés.'
      },
      intro: {
        title: 'Reporte mensual',
        body: 'La foto completa de un mes: ingresos, gasto por categoría, cada cuenta y tarjeta, y cómo avanzaron tus metas.'
      },
      month: {
        title: 'Elige un mes',
        body: 'Escoge cualquier mes y míralo, o descárgalo en PDF. El PDF se genera en el idioma que estés usando.'
      }
    },

    settings: {
      appearance: {
        title: 'Apariencia e idioma',
        body: 'Ambos están en el menú detrás de tus iniciales, arriba a la derecha. Apariencia ofrece claro, oscuro o seguir a tu sistema. El idioma cambia toda la aplicación, incluido el PDF del reporte mensual.'
      },
      replay: {
        title: 'Repetir estos recorridos',
        body: 'Volver a mostrar los recorridos reinicia todos, así cada pantalla te guía una vez más la próxima vez que la abras. Útil después de un tiempo sin usarla, o para enseñarle la aplicación a alguien más.'
      },
      intro: {
        title: 'Ajustes',
        body: 'Apariencia, idioma y los valores por defecto a los que recurre la aplicación.'
      },
      bankDefaults: {
        title: 'Cuentas por banco',
        body: 'Qué cuenta suponer cuando un banco no nombra ninguna. Una por banco y por dirección: el dinero que entra y el que sale pueden ser distintas.'
      }
    }
  },

  freelance: {
    title: 'Trabajo independiente',
    subtitle: 'Se paga cuando el cliente paga. Regístralo cuando ocurra.',
    emptyTitle: 'No hay trabajos registrados',
    emptyMessage: 'Agrega un trabajo para llevar control de lo acordado y de lo que realmente te han pagado.',

    newWork: 'Agregar trabajo independiente',
    addWork: 'Agregar trabajo',
    editWork: 'Editar trabajo independiente',
    workTitle: '¿Cuál es el trabajo?',
    workTitlePlaceholder: 'Rediseño de la página de inicio',
    client: 'Cliente',
    clientPlaceholder: 'Acme S.A.',
    clientHint: 'Opcional. Se cifra, igual que cualquier otro nombre que registres.',
    agreedAmount: 'Monto acordado',
    agreedAmountHint: 'Lo que esperas cobrar. La cifra real se registra por separado.',
    dueOn: 'Fecha de entrega acordada',
    dueOnHint: 'Opcional. No pasa nada en esta fecha: es solo tu recordatorio.',
    notesPlaceholder: 'Alcance, tarifa, cualquier cosa que convenga recordar',

    added: '{title} agregado.',
    updated: 'Trabajo actualizado.',
    deleted: 'Trabajo eliminado.',

    markDelivered: 'Marcar como entregado',
    markedDelivered: 'Marcado como entregado. No se movió dinero.',
    gotPaid: 'Me pagaron',

    recordPayment: 'Registrar pago',
    agreedWas: 'Acordado: {amount}',
    amountReceived: 'Monto recibido',
    amountReceivedHint: 'Lo que realmente llegó, aunque sea distinto de lo acordado.',
    depositTo: 'Entró en',
    depositToHint: 'El saldo sube ahora, y este dinero cuenta para lo que puedes gastar con seguridad.',
    paidOn: 'Fecha de pago',
    confirmPayment: 'Registrar pago',
    paymentRecorded: '{amount} depositados.',
    noAccountInCurrency: 'No tienes ninguna cuenta en {currency}. Agrega una antes de registrar este pago.',

    paidOnDate: 'pagado el {date}',
    dueBy: 'para el {date}',
    outstandingLabel: 'Aún te deben',

    deleteTitle: '¿Eliminar este trabajo?',
    deleteMessage: 'Elimina {title}. No cambia nada más.',
    deletePaidMessage: 'Elimina {title} y saca el pago de la cuenta en la que entró. Si el trabajo se canceló, usa cancelar en su lugar.',

    status: {
      InProgress: 'En curso',
      Delivered: 'Entregado',
      Paid: 'Pagado',
      Cancelled: 'Cancelado'
    }
  },

  theme: {
    label: 'Apariencia',
    light: 'Claro',
    dark: 'Oscuro',
    system: 'Según el sistema'
  },

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
    settings: 'Ajustes',
    expand: 'Expandir navegación',
    collapse: 'Contraer navegación',
    main: 'Principal',
    openMenu: 'Abrir navegación',
    closeMenu: 'Cerrar navegación',
    skipToContent: 'Ir al contenido'
  },

  common: {
    add: 'Agregar',
    save: 'Guardar',
    cancel: 'Cancelar',
    confirm: 'Confirmar',
    close: 'Cerrar',
    closeDialog: 'Cerrar diálogo',
    dismiss: 'Descartar',
    edit: 'Editar',
    remove: 'Quitar',
    delete: 'Eliminar',
    add: 'Agregar',
    create: 'Crear',
    continueLabel: 'Continuar',
    keepIt: 'Conservar',
    tryAgain: 'Reintentar',
    saveChanges: 'Guardar cambios',
    new: 'Nuevo',
    refresh: 'Actualizar',
    loading: 'Cargando…',
    saving: 'Guardando…',
    search: 'Buscar',
    filter: 'Filtrar',
    apply: 'Aplicar',
    clear: 'Limpiar',
    all: 'Todo',
    none: 'Ninguno',
    optional: 'Opcional',
    notes: 'Notas',
    name: 'Nombre',
    amount: 'Monto',
    date: 'Fecha',
    dateAndTime: 'Fecha y hora',
    category: 'Categoría',
    description: 'Descripción',
    type: 'Tipo',
    kind: 'Clase',
    status: 'Estado',
    currency: 'Moneda',
    total: 'Total',
    completed: 'Completadas',
    active: 'Activas',
    monthly: 'Mensual',
    remaining: 'Restante',
    paid: 'Pagado',
    location: 'Ubicación',
    overdue: 'Vencido',
    today: 'Hoy',
    tomorrow: 'Mañana',
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
    previousPage: 'Página anterior',
    nextPage: 'Página siguiente',
    pagination: 'Paginación',
    noResults: 'Nada que mostrar',
    notFoundHint: 'Puede que se haya eliminado, o que no sea tuyo.',
    goToDashboard: 'Ir al panel',
    notFoundPage: 'Esa página no existe.',
    account: 'Cuenta',
    chooseCategory: 'Elige una categoría',
    paidFrom: 'Pagado desde',
    paymentSource: 'Origen del pago',
    myAccounts: 'Una de mis cuentas',
    myAccountsNote: 'Retira y registra un movimiento',
    external: 'Externo',
    externalNote: 'Efectivo o pagó otra persona',
    noAccountsInCurrency: 'No hay cuentas en {currency} disponibles',
    screenFailedTitle: 'Esta pantalla tuvo un problema',
    screenFailedMessage:
      'El resto de la app sigue bien. Reintenta, o ve a otra sección.'
  },

  auth: {
    loginTitle: 'Iniciar sesión',
    loginSubtitle: 'Retoma donde quedó tu dinero.',
    registerTitle: 'Crea tu cuenta',
    registerSubtitle: 'Toma un minuto. Tus datos siguen siendo tuyos.',
    brandTagline: 'Tu dinero, mapeado. Las cifras se calculan, nunca se adivinan.',
    email: 'Correo electrónico',
    emailPlaceholder: 'tu@ejemplo.com',
    password: 'Contraseña',
    passwordPlaceholder: 'Al menos 8 caracteres',
    passwordHint: 'Mínimo 8 caracteres.',
    fullName: 'Nombre completo',
    fullNamePlaceholder: 'Ada Lovelace',
    country: 'País',
    countryPlaceholder: 'El Salvador',
    currencyHint: 'Todos los totales de WealthMap se muestran en esta moneda.',
    signIn: 'Iniciar sesión',
    signUp: 'Crear cuenta',
    createOne: 'Crea una',
    noAccount: '¿Aún no tienes cuenta?',
    haveAccount: '¿Ya tienes una cuenta?',
    acceptTerms: 'Acepto los {terms} y la {privacy}.',
    acceptTermsRequired: 'Debes aceptar los Términos del Servicio y la Política de Privacidad.'
  },

  legal: {
    privacy: 'Política de Privacidad',
    terms: 'Términos del Servicio',
    backToApp: '← Volver a WealthMap',
    betaNotice: 'WealthMap está en beta. Este documento describe lo que el software hace hoy y puede cambiar con él.'
  },

  dashboard: {
    title: 'Panel',
    greeting: 'Qué gusto verte, {name}',
    loadFailed: 'No se pudo cargar tu panel',
    available: 'Disponible',
    availableCredit: 'Crédito disponible',
    totalDebt: 'Deuda total',
    safeToSpend: 'Puedes gastar',
    safeToSpendHint: 'Lo que puedes gastar entre cuentas y tarjetas y aún pagar a tiempo',
    netWorth: 'Patrimonio neto',
    netWorthNote: 'lo disponible menos todo lo que debes',
    needsAttention: 'Requiere atención',
    thisMonth: 'Este mes',
    spendableCash: 'Efectivo disponible',
    incomingSalary: 'Salario por llegar',
    fallingDue: 'Por vencer',
    spentSoFar: 'Gastado hasta ahora',
    spendingAboveIncome: 'Gastas más de lo que ingresas',
    debtRatio: 'Ratio de deuda',
    upcoming: 'Próximos',
    next30Days: 'Próximos 30 días',
    nothingDue: 'Nada por vencer',
    nothingDueMessage: 'No hay pagos de tarjeta, deuda ni cuotas en los próximos 30 días.',
    totalsCover: 'Los totales cubren',
    goalsTitle: 'Metas',
    noGoalsTitle: 'Aún no hay metas',
    noGoalsMessage:
      'Define un objetivo y WealthMap calcula cuánto debes apartar cada mes.',
    createGoal: 'Crear una meta',
    behind: 'Atrasadas',
    viewAllGoals: 'Ver todas las metas'
  },

  accounts: {
    title: 'Cuentas',
    subtitle: 'Cada saldo que tienes, y dónde está.',
    newAccount: 'Nueva cuenta',
    editAccount: 'Editar cuenta',
    transfer: 'Transferir',
    transferTitle: 'Transferir entre cuentas',
    totalHeld: 'Total en cuentas',
    deposit: 'Depositar',
    bonus: 'Bonificación',
    withdraw: 'Retirar',
    blocked: 'Bloqueada para ahorro',
    block: 'Bloquear para ahorro',
    unblock: 'Desbloquear',
    blockAria: 'Bloquear cuenta para ahorro',
    unblockAria: 'Desbloquear cuenta',
    deleteAria: 'Eliminar cuenta',
    unblockedToast: '{name} desbloqueada.',
    blockedToast: '{name} bloqueada — los depósitos siguen funcionando, los retiros no.',
    accountUnblocked: 'Cuenta desbloqueada.',
    accountBlocked: 'Cuenta bloqueada para ahorro.',
    bankName: 'Banco',
    accountNumber: 'Número de cuenta',
    debitCard: 'Tarjeta de débito',
    debitCardHint: '¿Hay una tarjeta de débito para esta cuenta?',
    noDebitCard: 'Ninguna',
    physicalCard: 'Física',
    digitalCard: 'Digital',
    debitCardLastFour: 'Últimos 4 de la tarjeta',
    debitCardLastFourHint: 'Los 4 dígitos de la tarjeta — un número distinto al de la cuenta.',
    numberUnknown: 'número no registrado',
    updatedToast: 'Cuenta actualizada.',
    createdToast: '{name} está lista.',
    bankPlaceholder: 'BBVA',
    accountName: 'Nombre de la cuenta',
    accountNamePlaceholder: 'Cuenta del día a día',
    typeHint: 'Las cuentas de ahorro se pueden bloquear.',
    openingBalance: 'Saldo inicial',
    openingBalanceHint:
      'Lo que hay en la cuenta hoy. No se puede cambiar después — usa depósitos.',
    checking: 'Corriente',
    savings: 'Ahorro',
    balance: 'Saldo',
    currentBalance: 'Saldo actual',
    movements: 'Movimientos',
    movementsSubtitle: 'Más recientes primero — cada cambio de saldo queda registrado',
    allAccounts: 'Todas las cuentas',
    backToAccounts: 'Volver a cuentas',
    notFound: 'Cuenta no encontrada',
    noAccountsTitle: 'Aún no hay cuentas',
    noAccountsMessage:
      'El dinero siempre vive en una cuenta. Agrega la primera para empezar.',
    addFirst: 'Agregar una cuenta',
    createAccount: 'Crear cuenta',
    loadFailed: 'No se pudieron cargar tus cuentas',
    needTwoToTransfer: 'Necesitas dos cuentas para transferir',
    unblockToWithdraw: 'Desbloquea la cuenta para retirar',
    noMovementsTitle: 'Aún no hay movimientos',
    noMovementsMessage: 'Depósitos, retiros, transferencias y pagos aparecen aquí.',
    depositKindHint: 'El salario y las transferencias se registran automáticamente.',
    depositPlaceholder: 'Depósito en efectivo',
    withdrawPlaceholder: 'Supermercado',
    locationPlaceholder: 'Opcional — Cajero Reforma 222',
    withdrawHint:
      'Los retiros se registran como retiros de cajero; el efectivo deja de seguirse.',
    from: 'Desde',
    to: 'Hacia',
    chooseAccount: 'Elige una cuenta',
    pickSourceFirst: 'Elige primero el origen',
    sameCurrencyHint: 'Solo cuentas en {currency} — no hay conversión.',
    transferred: '{amount} movidos a {name}.',
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
    newCard: 'Nueva tarjeta de crédito',
    editCard: 'Editar tarjeta',
    deleteAria: 'Eliminar tarjeta',
    pay: 'Pagar',
    limit: 'Límite',
    newLimit: 'Nuevo límite',
    updateLimit: 'Actualizar límite de crédito',
    limitHint: 'No puede quedar por debajo de lo que debes actualmente.',
    currentlyOwed: 'Adeudado actualmente',
    limitUpdated: 'El límite ahora es {amount}.',
    available: 'Disponible',
    availableCredit: 'Crédito disponible',
    owed: 'Adeudado',
    owedOf: '{used} adeudados de {limit}',
    cardName: 'Nombre de la tarjeta',
    cardNamePlaceholder: 'Gold',
    creditLimit: 'Límite de crédito',
    interestRate: 'Tasa de interés anual',
    interest: 'Interés',
    paymentDueDay: 'Día de pago',
    clampsHint: 'Se ajusta en meses cortos.',
    statementCutoff: 'Corte del estado de cuenta',
    dueDay: 'Día de pago',
    dueDayWithNumber: 'Vence el día {day}',
    statementCloses: 'Corte',
    installments: 'Cuotas',
    addsToStatement: 'Suma al corte',
    plansAddToStatement: 'Los planes suman al corte que vence el {date}',
    noPlansTitle: 'No hay planes de cuotas en esta tarjeta',
    noPlansMessage: 'Los planes comprados con esta tarjeta aparecerán aquí con lo que cada uno suma al corte.',
    dueThisStatement: 'A pagar este corte',
    paidToast: '{paid} pagado — {owed} aún se debe.',
    limitToast: 'El límite ahora es {limit}.',
    nextStatement: 'Próximo corte',
    futureInstallments: 'Cuotas futuras',
    paymentDue: 'Fecha de pago',
    charges: 'Cargos',
    charged: 'Cargado',
    payments: 'Pagos',
    registerPayment: 'Registrar pago',
    registerPaymentTitle: 'Registrar un pago de tarjeta',
    paymentAmountHint: 'No puede exceder lo adeudado.',
    payAll: 'Pagar todo',
    paymentMade: '{paid} pagados — quedan {owed} por pagar.',
    notFound: 'Tarjeta no encontrada',
    noChargesTitle: 'No hay cargos en esta tarjeta',
    noChargesMessage:
      'Las compras pagadas con esta tarjeta, y los planes de cuotas en ella, aparecen aquí.',
    noPaymentsMessage:
      'Cuando registres un pago aparecerá aquí — incluidos los pagos en efectivo, que no tocan ninguna cuenta.',
    allCards: 'Todas las tarjetas',
    backToCards: 'Volver a tarjetas',
    noCardsTitle: 'Aún no hay tarjetas',
    noCardsMessage:
      'Agrega una tarjeta para seguir su saldo, crédito disponible y fecha de pago.',
    addFirst: 'Agregar una tarjeta',
    addCard: 'Agregar tarjeta',
    limitInUse: '{percent}% de tu límite en uso',
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
    newPurchase: 'Registrar compra',
    recordTitle: 'Registrar una compra',
    item: 'Artículo',
    kindPurchase: 'Compra',
    editTitle: 'Corregir compra',
    recordedToast: '{name} registrada.',
    updatedToast: '{name} corregida.',
    deletedToast: '{name} eliminada.',
    editAria: 'Corregir {name}',
    deleteAria: 'Eliminar {name}',
    deleteTitle: '¿Eliminar esta compra?',
    deleteMessage: '{name} por {amount} se eliminará, y el dinero que movió se devolverá.',
    deleteSecond: 'La compra y su movimiento se borran definitivamente — no quedará registro de que existió.',
    store: 'Comercio',
    method: 'Método',
    paymentMethod: 'Método de pago',
    debit: 'Débito',
    creditCard: 'Tarjeta de crédito',
    cash: 'Efectivo',
    noStore: 'Sin comercio',
    productName: '¿Qué compraste?',
    productPlaceholder: 'Supermercado',
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
    thisPage: 'Esta página',
    paidWith: 'Pagado con',
    debitNote: 'Retira de una cuenta',
    creditNote: 'Carga a una tarjeta',
    cashNote: 'Solo se registra — el efectivo no se rastrea',
    card: 'Tarjeta',
    chooseCard: 'Elige una tarjeta',
    noCards: 'Aún no hay tarjetas',
    chooseAccount: 'Elige una cuenta',
    noAccounts: 'Aún no hay cuentas',
    cashCurrencyHint: 'El efectivo no tiene cuenta de la cual heredar la moneda.'
  },

  payments: {
    title: 'Pagos',
    subtitle:
      'Todo lo que has pagado a tarjetas, deudas y planes de cuotas — venga de donde venga el dinero.',
    paid: 'Pagado',
    source: 'Origen',
    from: 'Desde',
    allTypes: 'Todos los tipos',
    fromAccount: 'Desde cuenta',
    cashOrThirdParty: 'Efectivo / terceros',
    emptyTitle: 'No hay pagos registrados',
    emptyMessage: 'Los pagos de cualquier origen aparecen aquí, incluido el efectivo.'
  },

  installments: {
    title: 'Cuotas',
    subtitle:
      'Planes sin interés. El precio completo se carga a la tarjeta de entrada, y luego se paga mes a mes.',
    newPlan: 'Nuevo plan',
    createPlan: 'Crear plan',
    planTitle: 'Nuevo plan de cuotas',
    productLabel: '¿Qué estás comprando?',
    productPlaceholder: 'Televisor',
    card: 'Tarjeta',
    chooseCard: 'Elige una tarjeta',
    noCards: 'Aún no hay tarjetas',
    cardHint: 'El precio completo se carga a esta tarjeta de inmediato.',
    totalPrice: 'Precio total',
    months: 'Meses',
    purchaseDate: 'Fecha de compra',
    interestFreeHint:
      'Sin interés. El último pago lleva el redondeo para que el plan sume exacto.',
    planCreated: '{name} dividido en {count} pagos.',
    remaining: 'Restante',
    stillToPay: 'Falta pagar',
    paidOff: 'Pagado',
    paidOf: '{paid} de {total}',
    fullyPaid: 'Totalmente pagado',
    payNext: 'Pagar siguiente',
    payNextTitle: 'Pagar la siguiente cuota',
    payInstallment: 'Pagar cuota',
    schedule: 'Calendario',
    scheduleSubtitle: 'Generado cuando se creó el plan',
    scheduled: 'Programada',
    allPlans: 'Todos los planes',
    backToPlans: 'Volver a planes',
    notFound: 'Plan no encontrado',
    paymentsLeft: '{remaining} de {total} pagos restantes · el último {date}',
    lastPayment: 'Último pago {date}',
    progress: 'Progreso',
    chargedTo: 'Cargado a',
    cardRemoved: 'Tarjeta ya no disponible',
    paidToast: 'Cuota pagada — quedan {remaining} pagos.',
    fullyPaidToast: '{name} está totalmente pagado.',
    splitToast: '{name} dividido en {count} pagos.',
    planKind: 'Plan de cuotas',
    loadFailed: 'No se pudieron cargar tus planes',
    emptyTitle: 'No hay planes de cuotas',
    emptyMessage:
      'Divide una compra en meses sin interés. La tarjeta se carga completa hoy.',
    nothingOutstanding: 'Nada pendiente',
    nothingCompleted: 'Nada completado aún',
    allPaidOff: 'Todos los planes están pagados.',
    completedHint: 'Los planes aparecen aquí cuando se paga la última cuota.',
    addFirst: 'Crear un plan',
    planSubtitle: '{count} pagos sin interés · comprado el {date}'
  },

  stores: {
    title: 'Comercios',
    subtitle:
      'Un catálogo compartido — todos ven todos los comercios, pero solo tú puedes editar los que agregaste.',
    newStore: 'Nuevo comercio',
    editStore: 'Editar comercio',
    storeName: 'Nombre del comercio',
    logoUrl: 'URL del logo',
    addedByYou: 'Agregado por ti',
    searchPlaceholder: 'Buscar por nombre o categoría',
    emptyTitle: 'Aún no hay comercios',
    emptyMessage: 'Agrega los lugares donde compras para que tus compras puedan señalarlos.',
    addFirst: 'Agregar un comercio',
    addStore: 'Agregar comercio',
    noMatches: 'Sin coincidencias',
    noMatchesFor: 'Nada en el catálogo coincide con “{term}”.',
    namePlaceholder: 'Walmart',
    logoPlaceholder: 'https://…',
    logoHint: 'Opcional. Debe ser una URL completa.',
    noStore: 'Sin comercio',
    loadFailed: 'No se pudo cargar el catálogo'
  },

  debts: {
    title: 'Deudas',
    subtitle: 'Préstamos y cualquier otra cosa que debas fuera de una tarjeta de crédito.',
    newDebt: 'Nueva deuda',
    editDebt: 'Editar deuda',
    deleteAria: 'Eliminar deuda',
    namePlaceholder: 'Préstamo del auto',
    originalAmount: 'Monto original',
    original: 'Original',
    stillOwed: 'Aún adeudado',
    sameAsOriginal: 'Igual al original',
    stillOwedHint: 'Solo si ya has pagado una parte.',
    monthlyPayment: 'Pago mensual',
    dueDay: 'Día de pago',
    clampsHint: 'Se ajusta en meses cortos.',
    remaining: 'Restante',
    monthly: 'Mensual',
    nothingFurtherDue: 'Nada más por pagar',
    repaidOf: '{paid} de {total} pagados',
    markDefaulted: 'Marcar como incumplida',
    defaultLabel: 'Incumplida',
    markDefaultedTitle: '¿Marcar {name} como incumplida?',
    markDefaultedMessage:
      'Sigue en tus totales. Registrar un pago después la devuelve a activa.',
    registerPayment: 'Registrar pago',
    registerPaymentTitle: 'Registrar un pago de deuda',
    paymentHint: 'No puede exceder lo que queda.',
    payAll: 'Pagar todo',
    defaultedHint: 'Pagar una deuda incumplida la devuelve a activa.',
    payments: 'Pagos',
    paymentsSubtitle: 'Desde una cuenta o externamente',
    paymentsEmpty:
      'Los pagos de esta deuda aparecen aquí, incluidos los hechos en efectivo.',
    allDebts: 'Todas las deudas',
    backToDebts: 'Volver a deudas',
    notFound: 'Deuda no encontrada',
    loadFailed: 'No se pudieron cargar tus deudas',
    emptyTitle: 'No hay deudas registradas',
    emptyMessage:
      'Registra un préstamo para verlo en tus totales, en lo que puedes gastar y en tus próximos vencimientos.',
    addFirst: 'Agregar una deuda',
    addDebt: 'Agregar deuda',
    deleteTitle: '¿Eliminar {name}?',
    deleteMessage: 'Esto elimina la deuda y su historial. No se puede deshacer.',
    deleted: '{name} eliminada.'
  },

  goals: {
    title: 'Metas',
    subtitle: 'Para qué estás ahorrando, y cuánto hace falta cada mes para lograrlo.',
    newSavingsGoal: 'Nueva meta de ahorro',
    newProductGoal: 'Nueva meta de producto',
    editGoal: 'Editar meta',
    deleteAria: 'Eliminar meta',
    savings: 'Ahorro',
    products: 'Productos',
    savingsPlaceholder: 'Fondo de emergencia',
    productPlaceholder: 'PlayStation 6',
    target: 'Objetivo',
    alreadySaved: 'Ya ahorrado',
    alreadySavedHint: 'Opcional — lo que ya has apartado para esto.',
    deadline: 'Fecha límite',
    noneSet: 'Sin definir',
    savingsDeadlineHint: 'Determina la cifra mensual y si vas al día.',
    productDeadlineHint: 'Opcional. Sin ella no hay un monto mensual requerido.',
    linkedAccount: 'Cuenta de ahorro vinculada',
    trackOnly: 'Ninguna — solo seguimiento',
    noSavingsAccount: 'No hay cuenta de ahorro en esta moneda',
    linkHint: 'Vincula una y aportar mueve dinero real a ella.',
    monthsLeft: 'Meses restantes',
    neededMonthly: 'Necesario al mes',
    contribute: 'Aportar',
    percentFunded: '{percent}% financiado',
    ofTarget: 'de {amount}',
    toGo: 'faltan {amount}',
    fullyFundedToast: '{name} está totalmente financiada.',
    addedToast: 'Agregado — {amount} ahorrados hasta ahora.',
    createGoal: 'Crear meta',
    linkedHint: 'Vinculada — aportar mueve dinero real a la cuenta de ahorro',
    trackedOnlyHint: 'Solo seguimiento — aportar no mueve dinero',
    contributeTitle: 'Aportar a la meta',
    moveFrom: 'Mover desde',
    chooseAccount: 'Elige una cuenta',
    noEligibleAccounts: 'No hay cuentas elegibles',
    fillIt: 'Completar',
    productNoMoney: 'Las metas de producto solo siguen el progreso — no mueven dinero.',
    noLinkedAccount: 'Esta meta no tiene cuenta vinculada, así que no se mueve dinero entre cuentas.',
    contributeHint:
      'Es una transferencia real a la cuenta de ahorro de la meta, registrada en ambos lados.',
    targetReached: 'Objetivo ya alcanzado',
    completed: 'Completadas',
    offPace: 'Fuera de ritmo',
    noSavingsGoals: 'Aún no hay metas de ahorro',
    noProductGoals: 'Aún no hay metas de producto',
    savingsEmptyMessage:
      'Define un objetivo y una fecha, y WealthMap calcula cuánto apartar cada mes.',
    productEmptyMessage:
      '¿Ahorras para algo específico? Regístralo aquí — la fecha límite es opcional.',
    deleteTitle: '¿Eliminar {name}?',
    deleteMessage:
      'La meta y su progreso se eliminan. El dinero que ya esté en una cuenta vinculada se queda.',
    deleted: '{name} eliminada.'
  },

  job: {
    title: 'Trabajo e ingresos',
    subtitle: 'Tu salario, lo que se le descuenta, y cualquier otra cosa que llegue con regularidad.',
    addJob: 'Agregar tu trabajo',
    saveJob: 'Guardar trabajo',
    editJob: 'Editar trabajo',
    deleteJob: 'Eliminar trabajo',
    jobTitle: 'Puesto',
    jobTitlePlaceholder: 'Desarrollador full-stack',
    employer: 'Empleador',
    employerPlaceholder: 'Acme',
    grossSalary: 'Salario bruto mensual',
    grossHint: 'Antes de deducciones.',
    paidInto: 'Se deposita en',
    chooseAccount: 'Elige una cuenta',
    noAccounts: 'Aún no hay cuentas',
    noAccountInCurrency: 'No hay cuenta en esta moneda',
    first: 'Primero',
    second: 'Segundo',
    third: 'Tercero',
    none: 'Ninguno',
    paymentDays: 'Días de pago',
    paymentDaysHint:
      'Entre uno y tres días al mes. Un día posterior al fin de mes se ajusta al último día.',
    deductionsAfterSave: 'Las deducciones se agregan después de guardar el trabajo.',
    grossMonthly: 'Bruto mensual',
    deducted: 'Deducido',
    netMonthly: 'Neto mensual',
    perDeposit: 'Por depósito (después de deducciones)',
    paidOnDay: 'Se paga el día',
    next: 'Próximos',
    deductions: 'Deducciones',
    deductionsSubtitle: 'Tomadas de tu planilla — la app hace la aritmética, no la ley fiscal',
    addDeduction: 'Agregar deducción',
    editDeduction: 'Editar deducción',
    deductionNamePlaceholder: 'Renta',
    deductionNameHint:
      'Cópiala de tu planilla — WealthMap hace la aritmética, no la ley fiscal.',
    percentage: 'Porcentaje',
    percentageOfGross: 'Porcentaje del bruto',
    fixedAmount: 'Monto fijo',
    takesOff: 'Descuenta',
    netBecomes: 'El neto mensual queda en',
    deductionsExceed: 'Las deducciones superarían tu salario bruto. Esto será rechazado.',
    perMonth: '{amount} al mes',
    nextPayday: 'Próximo:',
    perPayday: '{amount} en cada uno de {count} pagos',
    noDeductionsTitle: 'Sin deducciones',
    noDeductionsMessage: 'El neto es igual al bruto hasta que agregues lo que se descuenta.',
    deductionRemoved: 'Deducción eliminada.',
    removeDeductionTitle: '¿Quitar {name}?',
    removeDeductionMessage: 'Tu salario neto sube en ese monto.',
    otherIncome: 'Otros ingresos',
    otherIncomeSubtitle:
      'Extras recurrentes — el dinero puntual es un depósito de bonificación en una cuenta',
    noOtherIncome: 'Sin otros ingresos',
    noOtherIncomeMessage: 'Trabajo independiente, alquiler, cualquier cosa que llegue con horario.',
    addIncome: 'Agregar ingreso',
    addRecurringIncome: 'Agregar ingreso recurrente',
    editIncome: 'Editar ingreso',
    incomePlaceholder: 'Trabajo independiente',
    frequency: 'Frecuencia',
    countsAs: 'Cuenta como',
    perMonthInTotals: 'al mes en tus totales.',
    incomeHint:
      'Esto describe el ingreso esperado. El dinero solo aparece en una cuenta cuando registras un depósito.',
    monthlyEquivalent: 'Equivalente mensual',
    incomeRemoved: 'Ingreso eliminado.',
    removeIncomeTitle: '¿Quitar {name}?',
    jobDeleted: 'Trabajo eliminado.',
    deleteJobTitle: '¿Eliminar {name}?',
    deleteJobMessage:
      'Se eliminan el trabajo y todas sus deducciones. Tus cuentas quedan intactas.',
    emptyTitle: 'No hay trabajo registrado',
    emptyMessage:
      'Agrega tu salario y sus deducciones, y WealthMap calcula lo que realmente recibes y cuándo llega.'
  },

  reports: {
    title: 'Informe mensual',
    subtitle: 'Lo que entró, lo que salió, y dónde terminaste.',
    download: 'Descargar PDF',
    pdf: 'PDF',
    reportMonth: 'Mes del informe',
    view: 'Ver',
    month: 'Mes',
    spendingByCategory: 'Gasto por categoría',
    largestExpenses: 'Gastos más grandes',
    largestExpensesSubtitle: 'Las cinco compras individuales más grandes',
    income: 'Ingresos',
    spending: 'Gastos',
    netResult: 'Resultado neto',
    nothingToShow: 'Nada que mostrar',
    totalSpent: 'Total gastado',
    expectedNetSalary: 'Salario neto esperado',
    perMonth: 'al mes',
    noIncomeTitle: 'No hay ingresos registrados este mes',
    noIncomeMessage: 'Las transferencias entre tus propias cuentas no cuentan como ingreso.',
    accountsTitle: 'Cuentas',
    colAccount: 'Cuenta',
    colOpening: 'Inicial',
    colIn: 'Entradas',
    colOut: 'Salidas',
    colClosing: 'Final',
    colCard: 'Tarjeta',
    colCharged: 'Cargado',
    colPaid: 'Pagado',
    colOwed: 'Adeudado',
    colAvailable: 'Disponible',
    accountsSubtitle: 'De apertura a cierre, movimiento por movimiento',
    noAccountsTitle: 'No hay cuentas en este período',
    noAccountsMessage: 'Las cuentas abiertas después de este mes no se muestran.',
    cardsTitle: 'Tarjetas de crédito',
    cardsSubtitle: 'Los saldos son actuales, no de fin de mes. Lo pagado cubre todos los orígenes, incluido el efectivo.',
    downloaded: 'Informe descargado.',
    downloadFailed: 'No se pudo generar el PDF. Reintenta.',
    loadFailed: 'No se pudo generar el informe'
  },

  notifications: {
    title: 'Notificaciones',
    subtitle: 'Alertas que se te han mostrado. Volver a revisar reactiva lo que siga vigente.',
    markAllRead: 'Marcar todo como leído',
    markAsRead: 'Marcar como leída',
    checkNow: 'Revisar ahora',
    footnote: 'Marcar algo como leído es un acuse, no un silencio — si la condición sigue vigente, la próxima revisión la vuelve a levantar.',
    unread: '{count} sin leer',
    unreadTab: 'Sin leer',
    readTab: 'Leídas',
    nothingUnread: 'Nada sin leer',
    upToDate: 'Estás al día.',
    emptyTitle: 'Aún no hay notificaciones',
    emptyMessage:
      'Usa Revisar ahora para convertir tus alertas actuales en notificaciones que puedas atender.',
    markFailed: 'No se pudo marcar como leída.',
    checkFailed: 'No se pudo revisar si hay alertas nuevas.',
    nothingNew: 'Nada nuevo — todo lo vigente ya está aquí.',
    created: '{count} notificaciones nuevas.'
  },

/**
   * Valores que la API envía tal cual: nombres de enums y categorías del
   * catálogo. La clave es exactamente lo que devuelve el servidor, así que un
   * valor sin traducir cae en el texto del servidor y no en un vacío.
   */
  server: {
    movementType: {
      SalaryDeposit: 'Depósito de salario',
      Deposit: 'Depósito',
      Bonus: 'Bonificación',
      TransferIn: 'Transferencia recibida',
      TransferOut: 'Transferencia enviada',
      Purchase: 'Compra',
      Payment: 'Pago',
      AtmWithdrawal: 'Retiro de cajero'
    },
    paymentMethod: {
      DebitAccount: 'Débito',
      CreditCard: 'Tarjeta de crédito',
      Cash: 'Efectivo'
    },
    accountType: { Checking: 'Corriente', Savings: 'Ahorro' },
    debitCardType: { None: 'Sin tarjeta de débito', Physical: 'Tarjeta de débito', Digital: 'Tarjeta digital' },
    paymentTarget: { CreditCard: 'Tarjeta de crédito', Debt: 'Deuda', Installment: 'Cuota' },
    paymentSource: { Account: 'Desde cuenta', External: 'Efectivo / terceros' },
    goalStatus: {
      OnTrack: 'Al día',
      BehindSchedule: 'Atrasada',
      DeadlinePassed: 'Fecha vencida',
      Completed: 'Completada'
    },
    goalKind: { Savings: 'Ahorro', Product: 'Producto' },
    debtStatus: { Active: 'Activa', PaidOff: 'Pagada', Defaulted: 'Incumplida' },
    incomeFrequency: {
      Weekly: 'Semanal',
      Biweekly: 'Quincenal',
      Monthly: 'Mensual',
      Yearly: 'Anual'
    },
    dueKind: { CreditCard: 'Tarjeta de crédito', Debt: 'Deuda', Installment: 'Cuota' },
    severity: { Info: 'Información', Warning: 'Advertencia', Critical: 'Crítico' },
    deductionType: { Fixed: 'Monto fijo', Percentage: 'Porcentaje del bruto' },
    category: {
      Food: 'Comida',
      Groceries: 'Supermercado',
      Restaurants: 'Restaurantes',
      Transport: 'Transporte',
      Electronics: 'Electrónica',
      Clothing: 'Ropa',
      Health: 'Salud',
      Entertainment: 'Entretenimiento',
      Home: 'Hogar',
      Services: 'Servicios',
      Education: 'Educación',
      Travel: 'Viajes',
      Other: 'Otro'
    }
  },

  /** Texto de error del servidor, reconocido por su forma. Ver useServerText. */
  serverMessage: {
    validationFailed: 'Validación fallida',
    businessRule: 'Regla de negocio no cumplida',
    notFound: 'No encontrado',
    unauthorized: 'Necesitas iniciar sesión de nuevo.',
    insufficientFunds: "Fondos insuficientes en '{name}'. Disponible {available}, solicitado {requested}.",
    exceedsCredit: "Eso excede el crédito disponible en '{name}'.",
    alreadyArchived: 'Eso ya fue eliminado.',
    blockedAccount: "'{name}' está bloqueada para ahorro. Desbloquéala antes de sacar dinero.",
    futureDate: 'La fecha no puede estar en el futuro.',
    currencyMismatch: 'Las monedas no coinciden, y WealthMap no convierte entre ellas.'
  },

/**
   * Alertas, con la clave del AlertType que envía la API. Los marcadores se
   * llenan con las partes que el servidor manda junto a la frase, así las cifras
   * sobreviven a la traducción. Un tipo que falte aquí cae al inglés del servidor.
   */
  alert: {
    CardPaymentDueSoon: {
      title: "El pago de '{name}' vence en {daysUntil} día(s)",
      message: "Debes {amount} en '{name}', con vencimiento {dueDate}."
    },
    DebtPaymentDueSoon: {
      title: "El pago de '{name}' vence en {daysUntil} día(s)",
      message: "Vencen {amount} el {dueDate} por '{name}'."
    },
    InstallmentDueSoon: {
      title: "El pago de '{name}' vence en {daysUntil} día(s)",
      message: "Vencen {amount} el {dueDate} por '{name}'."
    },
    InsufficientBalanceForCardPayment: {
      title: 'El saldo en cuenta corriente no cubrirá los próximos pagos de tarjeta',
      message: 'Vencen {owed} en {days} días pero la cuenta corriente tiene {checking}.',
      canCover: ' Podrías mover {shortfall} desde ahorro para cubrirlo.',
      cannotCover: ' El ahorro tampoco cubriría la diferencia.'
    },
    HighDebtRatio: {
      title: 'Los pagos de deuda toman el {ratio}% de tus ingresos',
      message:
        'Pagos comprometidos de {obligations} frente a un ingreso neto de {income}. ' +
        'Por encima del {threshold}% queda poco margen.'
    },
    OverspendingVsIncome: {
      title: 'Los gastos superan los ingresos este mes',
      message: 'Has gastado {spent} este mes frente a un ingreso neto de {income}.'
    },
    GoalBehindSchedule: {
      title: "'{name}' va atrasada",
      message: "'{name}' está financiada al {progress}% y no lleva el ritmo necesario para su fecha límite."
    },
    GoalDeadlinePassed: {
      title: "'{name}' pasó su fecha límite",
      message: "'{name}' llegó a su fecha límite con {progress}% financiado. Define una nueva fecha o ajusta el objetivo."
    },
    GoalReached: {
      title: "'{name}' está totalmente financiada",
      message: "Alcanzaste el objetivo de '{name}'."
    }
  },

/** Frases que envuelven una cifra. El número es un marcador para poder mover el orden. */
  composed: {
    dashboardNote: 'Todo lo de abajo se calcula con lo que has registrado — nada se estima.',
    checkingAndSavings: '{checking} en corriente · {savings} en ahorro',
    usedOfLimit: '{used} usados de {limit} ({percent}%)',
    loansAndCards: '{loans} en préstamos · {cards} en tarjetas',
    includesInstallments: 'incluye {amount} en planes de cuotas',
    spendableCashNote: 'excluye cuentas bloqueadas para ahorro',
    spendableOnCards: 'gastable ya, se paga en la fecha límite',
    closesOn: 'cierra el {date}',
    closedOn: 'cerró el {date}',
    payBy: 'paga antes del {date}',
    notYetBilled: 'aún no facturado',
    plusFutureInstallments: 'más {amount} en cuotas futuras',
    installmentsSettled: { one: '{count} cuota saldada.', other: '{count} cuotas saldadas.' },
    planMeta: '{total} pagos · {remaining} restantes',
    inDays: { one: 'en {count} día', other: 'en {count} días' },
    beforeDate: 'antes del {date}',
    cardsLoansInstallments: 'tarjetas, préstamos y cuotas',
    safeUntil: 'seguro hasta el {date}',
    goalsOffPace: { one: '{count} meta fuera de ritmo', other: '{count} metas fuera de ritmo' },
    itemCount: { one: '{count} artículo', other: '{count} artículos' },
    andMore: 'y {count} más — ver notificaciones',
    showingRecent: 'Mostrando las {shown} más recientes de {total} compras en esta tarjeta.',
    nextDue: 'Próximo vencimiento {date}',
    dueOnDay: 'Vence el día {day} de cada mes',
    nextDate: ' · próximo {date}',
    newSavingsGoal: 'Nueva meta de ahorro',
    newProductGoal: 'Nueva meta de producto',
    createSavingsGoal: 'Crear una meta de ahorro',
    createProductGoal: 'Crear una meta de producto',
    installmentOf: 'Cuota {number} de {total}',
    dueOn: 'Vence {date}',
    perDepositTimes: 'Por depósito ({count}×)',
    reportPeriod: '{start} → {end} · todos los montos en {currency}',
    reportDueDay: 'Vence el día {day}',
    generatedNote: 'Generado {when} · las cifras cubren solo lo que está en {currency}'
  },

  settings: {
    title: 'Ajustes',
    subtitle: 'Cómo WealthMap trata tus cuentas y tarjetas.'
  },

  tracking: {
    lastFour: 'Últimos 4 dígitos',
    lastFourHint:
      'Los últimos 4 dígitos que aparecen en los correos de transacciones de tu banco. Se usarán para la sincronización automática futura.',
    mode: 'Modo de seguimiento',
    manual: 'Manual',
    manualHint: 'Yo registro las transacciones.',
    automatic: 'Automático',
    automaticHint: 'Reservado para la sincronización por correo del banco. Aún no disponible.',
    comingSoon: 'Próximamente'
  },

  bankDefaults: {
    title: 'Cuentas por defecto',
    explain:
      'Algunos bancos no indican de qué cuenta salió una transferencia. Estos valores se usarán cuando se active la sincronización automática.',
    add: 'Agregar',
    newTitle: 'Nueva cuenta por defecto',
    editTitle: 'Editar cuenta por defecto',
    bankName: 'Banco',
    accountNumber: 'Número de cuenta',
    debitCard: 'Tarjeta de débito',
    debitCardHint: '¿Hay una tarjeta de débito para esta cuenta?',
    noDebitCard: 'Ninguna',
    physicalCard: 'Física',
    digitalCard: 'Digital',
    debitCardLastFour: 'Últimos 4 de la tarjeta',
    debitCardLastFourHint: 'Los 4 dígitos de la tarjeta — un número distinto al de la cuenta.',
    numberUnknown: 'número no registrado',
    updatedToast: 'Cuenta actualizada.',
    createdToast: '{name} está lista.',
    bankNamePlaceholder: 'Banco Agrícola',
    direction: 'Dirección',
    directionHint: 'Entrada y salida se configuran por separado.',
    inbound: 'Entrada',
    outbound: 'Salida',
    account: 'Cuenta',
    chooseAccount: 'Elige una cuenta',
    saved: 'Cuenta por defecto guardada.',
    deleted: 'Cuenta por defecto eliminada.',
    deleteTitle: '¿Eliminar este valor por defecto?',
    deleteMessage:
      'Las transferencias de {bank} ya no tendrán una cuenta de respaldo cuando se active la sincronización.',
    deleteSecond: 'No se puede deshacer, pero puedes volver a agregarlo cuando quieras.',
    emptyTitle: 'Aún no hay valores por defecto',
    emptyMessage:
      'No pasa nada. Agrega uno solo si tu banco envía correos de transferencia que nunca nombran la cuenta.'
  },

  offline: {
    message: 'Estás sin conexión. Los cambios fallarán hasta que vuelva.'
  }
}
