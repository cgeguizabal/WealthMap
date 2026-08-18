using System.Globalization;

namespace WealthMap.Infrastructure.Reports;

/// <summary>
/// Every word the monthly report PDF prints, in one language.
/// </summary>
/// <remarks>
/// The client's translations live in <c>WealthMap_Front-End/src/i18n</c> and cannot
/// be reached from here: the PDF is rendered on the server, from data, with no
/// browser involved. So the report carries its own copy of the vocabulary it needs.
///
/// That is a second place translations live, and the honest risk is drift. It is
/// bounded deliberately — this holds only the ~45 strings the document actually
/// prints, not the app's 789. Where a phrase already exists on the Reports screen
/// the wording here matches it, so a user reading both sees the same words.
///
/// Passed to the generator per request rather than held in a field: the generator
/// is a singleton, and a mutable language on it would leak one user's locale into
/// another's report under concurrency.
/// </remarks>
public sealed class ReportText
{
    /// <summary>Drives month names and number formatting as well as the wording.</summary>
    public CultureInfo Culture { get; }

    private readonly IReadOnlyDictionary<string, string> _strings;
    private readonly IReadOnlyDictionary<string, string> _enums;

    private ReportText(
        CultureInfo culture,
        IReadOnlyDictionary<string, string> strings,
        IReadOnlyDictionary<string, string> enums)
    {
        Culture = culture;
        _strings = strings;
        _enums = enums;
    }

    /// <summary>
    /// Anything that is not recognisably Spanish gets English, rather than an
    /// error: a report in the wrong language still tells you what you spent.
    /// </summary>
    public static ReportText For(string? locale) =>
        locale?.StartsWith("es", StringComparison.OrdinalIgnoreCase) == true ? Spanish : English;

    /// <summary>The label, or the key itself when one is missing — never an exception.</summary>
    public string this[string key] => _strings.GetValueOrDefault(key, key);

    /// <summary>
    /// A value that came from the data rather than the layout: a movement type, a
    /// goal status, an account type, a payment method, a spending category.
    /// </summary>
    /// <remarks>
    /// Falls back to spacing out the PascalCase name — "BehindSchedule" becomes
    /// "Behind Schedule" — so an enum member added to the domain before it is added
    /// here still reads as something rather than as code.
    /// </remarks>
    public string Value(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        return _enums.TryGetValue(value, out var translated) ? translated : Humanize(value);
    }

    private static string Humanize(string value) =>
        string.Concat(value.Select((c, i) => i > 0 && char.IsUpper(c) ? $" {c}" : c.ToString()));

    private static readonly ReportText English = new(
        CultureInfo.GetCultureInfo("en-US"),
        new Dictionary<string, string>
        {
            ["title"] = "Monthly report",
            ["amountsIn"] = "All amounts in {0}",
            ["generated"] = "generated {0} UTC",
            ["page"] = "page",
            ["of"] = "of",

            ["income"] = "Income",
            ["spending"] = "Spending",
            ["netResult"] = "Net result",

            ["type"] = "Type",
            ["count"] = "Count",
            ["total"] = "Total",
            ["share"] = "Share",
            ["items"] = "Items",
            ["category"] = "Category",
            ["amount"] = "Amount",
            ["item"] = "Item",
            ["method"] = "Method",

            ["noIncome"] = "No income recorded this month.",
            ["expectedSalary"] = "Expected net salary {0} {1} per month.",

            ["spendingByCategory"] = "Spending by category",
            ["noPurchases"] = "No purchases recorded this month.",
            ["cashNote"] = "Cash withdrawn this month: {0} {1}. It left your accounts but is "
                         + "excluded from the net result — cash purchases already cover it.",

            ["largestExpenses"] = "Largest expenses",
            ["nothingToShow"] = "Nothing to show.",
            ["dateUtc"] = "Date (UTC)",

            ["accounts"] = "Accounts",
            ["noAccounts"] = "No accounts in this currency.",
            ["account"] = "Account",
            ["opening"] = "Opening",
            ["in"] = "In",
            ["out"] = "Out",
            ["closing"] = "Closing",
            ["movements"] = "{0} movement(s)",

            ["creditCards"] = "Credit cards",
            ["card"] = "Card",
            ["charged"] = "Charged",
            ["paid"] = "Paid",
            ["owed"] = "Owed",
            ["available"] = "Available",
            ["cardMeta"] = "Due day {0} · limit {1}",
            ["cardsNote"] = "Card balances are current, not month-end. Paid includes payments "
                          + "from any source, including cash and third parties.",

            ["goals"] = "Goals",
            ["goal"] = "Goal",
            ["kind"] = "Kind",
            ["saved"] = "Saved",
            ["target"] = "Target",
            ["progress"] = "Progress"
        },
        new Dictionary<string, string>
        {
            ["SalaryDeposit"] = "Salary", ["Deposit"] = "Deposit", ["Bonus"] = "Bonus",
            ["TransferIn"] = "Transfer in", ["TransferOut"] = "Transfer out",
            ["Purchase"] = "Purchase", ["Payment"] = "Payment", ["AtmWithdrawal"] = "ATM withdrawal",

            ["Checking"] = "Checking", ["Savings"] = "Savings",
            ["DebitAccount"] = "Debit", ["CreditCard"] = "Credit card", ["Cash"] = "Cash",

            ["OnTrack"] = "On track", ["BehindSchedule"] = "Behind schedule",
            ["DeadlinePassed"] = "Deadline passed", ["Completed"] = "Completed",
            ["Product"] = "Product",

            ["Food"] = "Food", ["Groceries"] = "Groceries", ["Transport"] = "Transport",
            ["Electronics"] = "Electronics", ["Clothing"] = "Clothing", ["Health"] = "Health",
            ["Entertainment"] = "Entertainment", ["Home"] = "Home", ["Services"] = "Services",
            ["Education"] = "Education", ["Travel"] = "Travel", ["Other"] = "Other"
        });

    private static readonly ReportText Spanish = new(
        CultureInfo.GetCultureInfo("es-419"),
        new Dictionary<string, string>
        {
            ["title"] = "Informe mensual",
            ["amountsIn"] = "Todos los montos en {0}",
            ["generated"] = "generado {0} UTC",
            ["page"] = "página",
            ["of"] = "de",

            ["income"] = "Ingresos",
            ["spending"] = "Gastos",
            ["netResult"] = "Resultado neto",

            ["type"] = "Tipo",
            ["count"] = "Cantidad",
            ["total"] = "Total",
            ["share"] = "Proporción",
            ["items"] = "Artículos",
            ["category"] = "Categoría",
            ["amount"] = "Monto",
            ["item"] = "Artículo",
            ["method"] = "Método",

            ["noIncome"] = "No se registraron ingresos este mes.",
            ["expectedSalary"] = "Salario neto esperado: {0} {1} por mes.",

            ["spendingByCategory"] = "Gastos por categoría",
            ["noPurchases"] = "No se registraron compras este mes.",
            ["cashNote"] = "Efectivo retirado este mes: {0} {1}. Salió de tus cuentas pero se "
                         + "excluye del resultado neto — las compras en efectivo ya lo cubren.",

            ["largestExpenses"] = "Mayores gastos",
            ["nothingToShow"] = "Nada que mostrar.",
            ["dateUtc"] = "Fecha (UTC)",

            ["accounts"] = "Cuentas",
            ["noAccounts"] = "No hay cuentas en esta moneda.",
            ["account"] = "Cuenta",
            ["opening"] = "Saldo inicial",
            ["in"] = "Entradas",
            ["out"] = "Salidas",
            ["closing"] = "Saldo final",
            ["movements"] = "{0} movimiento(s)",

            ["creditCards"] = "Tarjetas de crédito",
            ["card"] = "Tarjeta",
            ["charged"] = "Cargado",
            ["paid"] = "Pagado",
            ["owed"] = "Adeudado",
            ["available"] = "Disponible",
            ["cardMeta"] = "Vence el día {0} · límite {1}",
            ["cardsNote"] = "Los saldos de las tarjetas son actuales, no de fin de mes. Pagado "
                          + "incluye pagos de cualquier origen, incluyendo efectivo y terceros.",

            ["goals"] = "Metas",
            ["goal"] = "Meta",
            ["kind"] = "Clase",
            ["saved"] = "Ahorrado",
            ["target"] = "Objetivo",
            ["progress"] = "Progreso"
        },
        new Dictionary<string, string>
        {
            ["SalaryDeposit"] = "Salario", ["Deposit"] = "Depósito", ["Bonus"] = "Bonificación",
            ["TransferIn"] = "Transferencia recibida", ["TransferOut"] = "Transferencia enviada",
            ["Purchase"] = "Compra", ["Payment"] = "Pago", ["AtmWithdrawal"] = "Retiro en cajero",

            ["Checking"] = "Corriente", ["Savings"] = "Ahorro",
            ["DebitAccount"] = "Débito", ["CreditCard"] = "Tarjeta de crédito", ["Cash"] = "Efectivo",

            ["OnTrack"] = "En camino", ["BehindSchedule"] = "Atrasada",
            ["DeadlinePassed"] = "Fecha límite vencida", ["Completed"] = "Completada",
            ["Product"] = "Producto",

            ["Food"] = "Comida", ["Groceries"] = "Supermercado", ["Transport"] = "Transporte",
            ["Electronics"] = "Electrónicos", ["Clothing"] = "Ropa", ["Health"] = "Salud",
            ["Entertainment"] = "Entretenimiento", ["Home"] = "Hogar", ["Services"] = "Servicios",
            ["Education"] = "Educación", ["Travel"] = "Viajes", ["Other"] = "Otros"
        });
}
