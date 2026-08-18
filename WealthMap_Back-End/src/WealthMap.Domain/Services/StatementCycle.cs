using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Services;

/// <summary>
/// How a card's balance divides between the statement that has closed and the
/// cycle still open.
/// </summary>
/// <param name="StatementBalance">Due on the next payment date.</param>
/// <param name="CurrentCycleCharges">Spent since the cutoff; billed on the next statement.</param>
/// <param name="FutureInstallments">Plan balance beyond this cycle's installment.</param>
/// <param name="LastCutoffDate">When the closed statement closed.</param>
public readonly record struct StatementSplit(
    Money StatementBalance,
    Money CurrentCycleCharges,
    Money FutureInstallments,
    DateOnly LastCutoffDate);

/// <summary>
/// Splits what is owed into what must be paid now and what merely exists.
/// </summary>
/// <remarks>
/// "Owed" on its own is not actionable. A $100 balance where $50 closed on the
/// last statement and $50 was spent yesterday means $50 is due on the 15th and
/// the rest is not due for another month — the same total, two very different
/// obligations.
///
/// The card stores one running <c>UsedCredit</c> and no statement history, so the
/// division is reconstructed from the dates on the charges that produced it.
/// </remarks>
public static class StatementCycle
{
    /// <summary>The most recent cutoff on or before <paramref name="today"/>.</summary>
    public static DateOnly LastCutoff(DateOnly today, int cutoffDay)
    {
        var thisMonth = PaymentSchedule.ClampToMonth(today.Year, today.Month, cutoffDay);

        if (thisMonth <= today) return thisMonth;

        var (year, month) = today.Month == 1 ? (today.Year - 1, 12) : (today.Year, today.Month - 1);

        return PaymentSchedule.ClampToMonth(year, month, cutoffDay);
    }

    /// <param name="usedCredit">The card's authoritative balance.</param>
    /// <param name="chargedSinceCutoff">Ordinary purchases dated after the cutoff.</param>
    /// <param name="installmentRemaining">All unpaid installments on this card.</param>
    /// <param name="installmentDueThisCycle">Installments falling due on or before the payment date.</param>
    /// <remarks>
    /// Payments are not read. They are inferred: whatever is still owed *is* the
    /// unpaid part, and money pays the oldest debt first, so what survives is the
    /// newest. Taking the current cycle as the smaller of "charged since the cutoff"
    /// and "still owed" gets that right without a payment ledger — pay the statement
    /// off and the figure correctly leaves only the new spending behind.
    ///
    /// It is also what makes an incomplete history safe. Any balance the purchase
    /// records cannot explain — an opening balance, a charge from before the app —
    /// falls into the statement rather than the open cycle, which is the older and
    /// more urgent of the two readings.
    /// </remarks>
    public static StatementSplit Split(
        DateOnly today,
        int cutoffDay,
        Money usedCredit,
        Money chargedSinceCutoff,
        Money installmentRemaining,
        Money installmentDueThisCycle)
    {
        var zero = Money.Zero(usedCredit.Currency);
        var lastCutoff = LastCutoff(today, cutoffDay);

        // Plans charge the card in full on day one but are paid monthly, so their
        // outstanding balance is not part of any single statement. It is set aside
        // here and only this cycle's installment is added back.
        var revolving = usedCredit - installmentRemaining;
        if (revolving.Amount < 0) revolving = zero;

        var currentCycle = chargedSinceCutoff.Amount < revolving.Amount ? chargedSinceCutoff : revolving;
        if (currentCycle.Amount < 0) currentCycle = zero;

        var dueThisCycle = installmentDueThisCycle.Amount > installmentRemaining.Amount
            ? installmentRemaining
            : installmentDueThisCycle;

        var statement = revolving - currentCycle + dueThisCycle;

        return new StatementSplit(
            statement,
            currentCycle,
            installmentRemaining - dueThisCycle,
            lastCutoff);
    }
}
