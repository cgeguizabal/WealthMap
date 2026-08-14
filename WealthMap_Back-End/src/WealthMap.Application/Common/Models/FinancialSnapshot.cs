using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Services;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Common.Models;

public record UpcomingDueDate(
    string Kind,
    Guid EntityId,
    string Name,
    DateOnly DueDate,
    int DaysUntil,
    decimal Amount);

/// <summary>
/// Everything the dashboard and the alert rules need, loaded once and aggregated
/// in the user's primary currency. Holdings in other currencies are excluded from
/// the totals (no FX rates in this app) and reported in <see cref="ExcludedCurrencies"/>.
/// </summary>
public sealed class FinancialSnapshot
{
    public const int DueSoonDays = 7;

    public string Currency { get; }
    public DateOnly Today { get; }

    private readonly IReadOnlyList<Account> _accounts;
    private readonly IReadOnlyList<CreditCard> _cards;
    private readonly IReadOnlyList<Debt> _debts;
    private readonly IReadOnlyList<InstallmentPurchase> _installments;
    private readonly IReadOnlyList<Purchase> _monthPurchases;

    public IReadOnlyList<SavingsGoal> SavingsGoals { get; }
    public IReadOnlyList<ProductGoal> ProductGoals { get; }
    public IReadOnlyList<CreditCard> Cards => _cards;
    public IReadOnlyList<Debt> Debts => _debts;
    public IReadOnlyList<InstallmentPurchase> Installments => _installments;

    private readonly Job? _job;
    private readonly IReadOnlyList<AdditionalIncome> _additionalIncomes;

    public FinancialSnapshot(
        string currency,
        DateOnly today,
        IReadOnlyList<Account> accounts,
        IReadOnlyList<CreditCard> cards,
        IReadOnlyList<Debt> debts,
        IReadOnlyList<InstallmentPurchase> installments,
        IReadOnlyList<SavingsGoal> savingsGoals,
        IReadOnlyList<ProductGoal> productGoals,
        Job? job,
        IReadOnlyList<AdditionalIncome> additionalIncomes,
        IReadOnlyList<Purchase> monthPurchases)
    {
        Currency = currency;
        Today = today;

        // Only same-currency holdings can be summed; the rest are surfaced separately.
        _accounts = accounts.Where(a => a.Balance.Currency == currency).ToList();
        _cards = cards.Where(c => c.CreditLimit.Currency == currency).ToList();
        _debts = debts.Where(d => d.OriginalAmount.Currency == currency).ToList();
        _installments = installments.Where(i => i.TotalPrice.Currency == currency).ToList();
        _monthPurchases = monthPurchases.Where(p => p.Amount.Currency == currency).ToList();
        SavingsGoals = savingsGoals.Where(g => g.TargetAmount.Currency == currency).ToList();
        ProductGoals = productGoals.Where(g => g.TargetAmount.Currency == currency).ToList();
        _job = job?.GrossMonthlySalary.Currency == currency ? job : null;
        _additionalIncomes = additionalIncomes.Where(i => i.Amount.Currency == currency).ToList();

        ExcludedCurrencies = accounts.Select(a => a.Balance.Currency)
            .Concat(cards.Select(c => c.CreditLimit.Currency))
            .Concat(debts.Select(d => d.OriginalAmount.Currency))
            .Where(c => c != currency)
            .Distinct()
            .Order()
            .ToList();
    }

    public IReadOnlyList<string> ExcludedCurrencies { get; }

    private Money Zero => Money.Zero(Currency);

    private Money Sum(IEnumerable<Money> values) => values.Aggregate(Zero, (a, b) => a + b);

    public Money TotalAvailable => Sum(_accounts.Select(a => a.Balance));

    public Money TotalInChecking =>
        Sum(_accounts.Where(a => a.Type == AccountType.Checking).Select(a => a.Balance));

    public Money TotalInSavings =>
        Sum(_accounts.Where(a => a.Type == AccountType.Savings).Select(a => a.Balance));

    public Money TotalCreditLimit => Sum(_cards.Select(c => c.CreditLimit));
    public Money TotalUsedCredit => Sum(_cards.Select(c => c.UsedCredit));
    public Money TotalAvailableCredit => TotalCreditLimit - TotalUsedCredit;

    public Money TotalLoanDebt =>
        Sum(_debts.Where(d => d.Status != DebtStatus.PaidOff).Select(d => d.RemainingAmount));

    /// <summary>Included in <see cref="TotalUsedCredit"/> — installments are charged to a card.</summary>
    public Money InstallmentRemaining =>
        Sum(_installments.Where(i => !i.IsCompleted).Select(i => i.RemainingBalance));

    public Money TotalDebt => TotalUsedCredit + TotalLoanDebt;

    public Money NetWorth => TotalAvailable - TotalDebt;

    public Money MonthlyNetIncome
    {
        get
        {
            var salary = _job?.NetMonthly ?? Zero;
            var extras = Sum(_additionalIncomes.Select(i => IncomeMath.ToMonthly(i.Amount, i.Frequency)));
            return salary + extras;
        }
    }

    /// <summary>
    /// Committed outflows for the month: loan payments plus the next installment
    /// of each active plan. Revolving card balances are excluded — the user chooses
    /// how much of those to pay.
    /// </summary>
    public Money MonthlyObligations
    {
        get
        {
            var loans = Sum(_debts.Where(d => d.Status != DebtStatus.PaidOff).Select(d => d.MonthlyPayment));

            var installments = Sum(_installments
                .Where(i => !i.IsCompleted)
                .Select(i => i.NextUnpaid().Amount));

            return loans + installments;
        }
    }

    /// <summary>
    /// Cash the user can actually reach. Accounts blocked for saving are excluded:
    /// the domain refuses to withdraw from them, so counting that money as
    /// spendable would be a promise the app itself would break.
    /// </summary>
    public Money SpendableCash =>
        Sum(_accounts.Where(a => !a.IsBlockedForSaving).Select(a => a.Balance));

    /// <summary>
    /// Walks the calendar forward to the furthest upcoming due date, adding salary
    /// as it lands and subtracting bills as they fall due.
    /// </summary>
    /// <remarks>
    /// Computed once and reused: every figure below reads from it, and rebuilding
    /// the timeline per property would repeat the same date arithmetic four times.
    /// </remarks>
    private LiquidityForecast? _forecast;

    public LiquidityForecast Forecast => _forecast ??= LiquidityProjection.Forecast(
        Today, SpendableCash, _job, _cards, _debts, _installments);

    /// <summary>
    /// What can be spent today without being short when a bill actually falls due.
    /// </summary>
    /// <remarks>
    /// The lowest point of the projected balance, not a subtraction of today's
    /// figures. Holding $500 against a $1,400 card bill is fine if two $500 paydays
    /// land first, and is not fine if the bill is due tomorrow — same balance, same
    /// bill, opposite answers. Only the dates separate them.
    /// </remarks>
    public Money SafeToSpend => Forecast.SafeToSpend;

    /// <summary>The date the projection has to reach: the furthest bill it covers.</summary>
    public DateOnly SafeToSpendHorizon => Forecast.Horizon;

    /// <summary>Salary expected to land before the horizon.</summary>
    public Money IncomingBeforeHorizon => Forecast.IncomingBeforeHorizon;

    /// <summary>Everything falling due before the horizon.</summary>
    public Money CommittedBeforeHorizon => Forecast.CommittedBeforeHorizon;

    public Money MonthSpending => Sum(_monthPurchases.Select(p => p.Amount));

    /// <summary>Obligations as a share of net income; null when there is no income on record.</summary>
    public decimal? DebtRatio => MonthlyNetIncome.IsZero
        ? null
        : decimal.Round(MonthlyObligations.Amount / MonthlyNetIncome.Amount * 100m, 2, MidpointRounding.AwayFromZero);

    public IReadOnlyList<UpcomingDueDate> UpcomingDueDates(int withinDays)
    {
        var results = new List<UpcomingDueDate>();

        foreach (var card in _cards.Where(c => !c.UsedCredit.IsZero))
        {
            var due = IncomeMath.NextOccurrence(Today, card.PaymentDueDay);
            var days = due.DayNumber - Today.DayNumber;

            if (days <= withinDays)
                results.Add(new UpcomingDueDate(
                    "CreditCard", card.Id, card.CardName, due, days, card.UsedCredit.Amount));
        }

        foreach (var debt in _debts.Where(d => d.Status != DebtStatus.PaidOff))
        {
            var due = IncomeMath.NextOccurrence(Today, debt.MonthlyDueDay);
            var days = due.DayNumber - Today.DayNumber;

            if (days <= withinDays)
                results.Add(new UpcomingDueDate(
                    "Debt", debt.Id, debt.Name, due, days, debt.MonthlyPayment.Amount));
        }

        foreach (var plan in _installments.Where(i => !i.IsCompleted))
        {
            var next = plan.NextUnpaid();
            var days = next.DueDate.DayNumber - Today.DayNumber;

            if (days <= withinDays)
                results.Add(new UpcomingDueDate(
                    "Installment", plan.Id, plan.ProductName, next.DueDate, days, next.Amount.Amount));
        }

        return results.OrderBy(r => r.DueDate).ToList();
    }
}
