using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Services;

/// <summary>One dated change to the user's cash: salary in, an obligation out.</summary>
public readonly record struct CashEvent(DateOnly Date, Money Amount, string Kind, string Label);

/// <summary>
/// The result of walking the calendar forward from today.
/// </summary>
/// <param name="SafeToSpend">
/// The lowest the balance ever gets between now and <paramref name="Horizon"/>.
/// Spending more than this today means being short on some later date, even if
/// today's balance looks comfortable.
/// </param>
/// <param name="Horizon">The furthest obligation the projection had to cover.</param>
/// <param name="LowestOn">The date the low point falls on — the day that binds.</param>
public readonly record struct LiquidityForecast(
    Money SafeToSpend,
    DateOnly Horizon,
    DateOnly LowestOn,
    Money IncomingBeforeHorizon,
    Money CommittedBeforeHorizon,
    IReadOnlyList<CashEvent> Events);

/// <summary>
/// Works out how much can be spent today — across accounts and cards together —
/// without leaving the user short when a card or loan actually falls due.
/// </summary>
/// <remarks>
/// A single subtraction cannot answer this, because money and bills do not arrive
/// on the same day. Holding $500 against a $1,400 card bill looks hopeless until
/// two $500 paydays land before the due date — and looks fine if the bill is due
/// tomorrow. The order of the dates is the whole answer, so this walks them.
///
/// Spending capacity is not capped at cash on hand. A card charged today is not
/// settled until its statement falls due, so the real question is whether the
/// balance can be cleared *then*, out of everything that has arrived by then.
/// That is what makes this a spending limit for cards and accounts together
/// rather than a cash balance.
///
/// The figure is the *minimum* running balance from that settlement date onward,
/// not the closing one. A balance that dips below zero in the middle and recovers
/// by the end is still a missed payment on the day it dipped.
/// </remarks>
public static class LiquidityProjection
{
    /// <summary>
    /// Nothing further out is projected, however long the obligations run. Beyond a
    /// couple of months the inputs are guesses — salary is assumed unchanged, no new
    /// spending is assumed at all — and a figure built on guesses should not be
    /// presented as a safe amount.
    /// </summary>
    private const int MaxHorizonDays = 75;

    /// <summary>
    /// When today's card balance actually has to be paid.
    /// </summary>
    /// <remarks>
    /// Not the next due day: a card charged today is billed on the statement that
    /// closes at the next cutoff, and that statement is due on the following due
    /// day. On the 15th, with a cutoff late in the month and payment due on the
    /// 17th, the money is not needed until the 17th of *next* month — which is
    /// most of the runway this calculation exists to find.
    ///
    /// Simplification worth knowing: a balance carried from a statement that has
    /// already closed is due sooner than this says. Separating the two needs
    /// statement history the app does not keep, so the later date is used
    /// throughout.
    /// </remarks>
    public static DateOnly StatementDueDate(DateOnly today, int cutoffDay, int dueDay)
    {
        var cutoff = IncomeMath.NextOccurrence(today, cutoffDay);

        // Strictly after the cutoff: a due day landing on the cutoff itself belongs
        // to the cycle that is closing, not the one that just opened.
        return IncomeMath.NextOccurrence(cutoff.AddDays(1), dueDay);
    }

    public static LiquidityForecast Forecast(
        DateOnly today,
        Money spendableCash,
        Job? job,
        IReadOnlyList<CreditCard> cards,
        IReadOnlyList<Debt> debts,
        IReadOnlyList<InstallmentPurchase> installments)
    {
        var zero = Money.Zero(spendableCash.Currency);
        var live = cards.Where(c => !c.IsArchived).ToList();

        // When money spent today actually has to be paid. On a card that is the
        // statement due date, which is why spending capacity is not limited to cash
        // on hand: a purchase now is settled weeks later, out of salary that has
        // landed in the meantime. Cash is due immediately, so with no card the date
        // is today and the answer collapses back to "what you hold".
        var spendDueOn = live.Count == 0
            ? today
            : live.Max(c => StatementDueDate(today, c.StatementCutoffDay, c.PaymentDueDay));

        var outflows = BuildOutflows(today, zero, live, debts, installments);

        var horizon = outflows.Count == 0
            ? spendDueOn
            : Max(outflows.Max(e => e.Date), spendDueOn);

        var latestAllowed = today.AddDays(MaxHorizonDays);

        if (horizon > latestAllowed)
        {
            horizon = latestAllowed;
            outflows = outflows.Where(e => e.Date <= horizon).ToList();
        }

        var events = outflows
            .Concat(BuildSalary(today, horizon, job))
            .OrderBy(e => e.Date)
            // Money in before money out on the same day. Salary landing on the
            // 15th can pay a card due on the 15th — that is how a person actually
            // does it, and treating the two as if the payment cleared first would
            // reserve a whole payday against a bill the salary covers.
            //
            // The assumption is that the deposit arrives before the payment is
            // made, which holds when the user pays the bill themselves. A direct
            // debit taken at midnight against a salary posted at noon would break
            // it, and that case would need the two ordered the other way.
            .ThenByDescending(e => e.Amount.Amount)
            .ToList();

        // Only the stretch from spendDueOn onward constrains new spending. A dip
        // before then is money already committed and already accounted for — it
        // cannot be made worse by a purchase that is not billed until later.
        var running = spendableCash;
        Money? lowest = null;
        var lowestOn = spendDueOn;

        foreach (var e in events)
        {
            // The balance on spendDueOn binds even when no event lands on it —
            // that is the day money spent today comes due. Sampling only on event
            // dates skipped it entirely, and a £10 card charge could appear to
            // cost a whole payday.
            //
            // Strictly after, not on: an event on spendDueOn is sampled by the
            // loop below, once that day's salary has been added.
            if (lowest is null && e.Date > spendDueOn)
                lowest = running;

            running += e.Amount;

            if (e.Date < spendDueOn) continue;

            if (lowest is null || running.Amount < lowest.Value.Amount)
            {
                lowest = running;
                lowestOn = e.Date;
            }
        }

        // No event falls on or after the due date, so nothing can pull the balance
        // down after it: what has accumulated by then is the answer.
        var headroom = lowest ?? running;

        // Capacity is still bounded by the instruments themselves: a card cannot be
        // charged past its limit however comfortably it could be repaid.
        var ceiling = spendableCash + AvailableCredit(zero, live);

        if (headroom.Amount > ceiling.Amount)
        {
            headroom = ceiling;
            lowestOn = today;
        }

        var incoming = Sum(zero, events.Where(e => e.Amount.Amount > 0));
        var committed = Sum(zero, events.Where(e => e.Amount.Amount < 0));

        return new LiquidityForecast(
            headroom, horizon, lowestOn, incoming, -committed, events);
    }

    private static Money AvailableCredit(Money zero, IEnumerable<CreditCard> cards) =>
        cards.Aggregate(zero, (sum, c) =>
            c.CreditLimit.Amount > c.UsedCredit.Amount ? sum + (c.CreditLimit - c.UsedCredit) : sum);

    private static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;

    private static List<CashEvent> BuildOutflows(
        DateOnly today,
        Money zero,
        IReadOnlyList<CreditCard> cards,
        IReadOnlyList<Debt> debts,
        IReadOnlyList<InstallmentPurchase> installments)
    {
        var events = new List<CashEvent>();

        foreach (var card in cards.Where(c => !c.UsedCredit.IsZero))
        {
            // A plan charges the card in full on day one, so the plan's outstanding
            // balance sits inside UsedCredit — but the user pays it monthly, not at
            // once. Only the revolving remainder is due on the statement; the
            // installments are added below on their own due dates.
            var planned = installments
                .Where(i => i.CreditCardId == card.Id && !i.IsCompleted)
                .Aggregate(zero, (sum, i) => sum + i.RemainingBalance);

            var revolving = card.UsedCredit - planned;

            if (revolving.Amount > 0)
                events.Add(new CashEvent(
                    StatementDueDate(today, card.StatementCutoffDay, card.PaymentDueDay),
                    -revolving, "CreditCard", card.CardName));
        }

        foreach (var plan in installments.Where(i => !i.IsCompleted))
        {
            var next = plan.NextUnpaid();
            events.Add(new CashEvent(next.DueDate, -next.Amount, "Installment", plan.ProductName));
        }

        foreach (var debt in debts.Where(d => d.Status != DebtStatus.PaidOff))
        {
            // Never reserve more than is actually left to repay.
            var due = debt.MonthlyPayment.Amount > debt.RemainingAmount.Amount
                ? debt.RemainingAmount
                : debt.MonthlyPayment;

            events.Add(new CashEvent(
                IncomeMath.NextOccurrence(today, debt.MonthlyDueDay),
                -due, "Debt", debt.Name));
        }

        return events;
    }

    private static IEnumerable<CashEvent> BuildSalary(DateOnly today, DateOnly horizon, Job? job)
    {
        if (job is null) yield break;

        // From tomorrow: a payday that already landed is in the balance, and counting
        // it again would invent money.
        foreach (var payday in job.ScheduledDatesBetween(today.AddDays(1), horizon))
            yield return new CashEvent(payday, job.NetForPayday(payday), "Salary", job.Employer);
    }

    private static Money Sum(Money zero, IEnumerable<CashEvent> events) =>
        events.Aggregate(zero, (sum, e) => sum + e.Amount);
}
