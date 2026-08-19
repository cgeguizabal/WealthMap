using WealthMap.Domain.Entities;
using WealthMap.Domain.Services;

namespace WealthMap.Application.Features.CreditCards.DTOs;

/// <param name="NextCutoffDate">When the current statement closes.</param>
/// <param name="NextDueDate">
/// When today's balance has to be paid. Not simply the next occurrence of
/// <paramref name="PaymentDueDay"/>: spending is billed on the statement closing at
/// the next cutoff, and that statement is due on the following due day. This is the
/// same date the dashboard's safe-to-spend projection reserves against.
/// </param>
/// <param name="StatementBalance">
/// Of <paramref name="UsedCredit"/>, the part due on <paramref name="NextDueDate"/>.
/// </param>
/// <param name="CurrentCycleCharges">
/// Spent since <paramref name="LastCutoffDate"/>. Not due until the cycle after next.
/// </param>
/// <param name="FutureInstallments">
/// Plan balance beyond this cycle's installment. Owed, but not on any statement yet.
/// </param>
/// <param name="BlockedOn">
/// When the card was reported lost, stolen, damaged or compromised. Null while it is
/// in service. A blocked card still owes what it owes and still falls due on its
/// usual day; what stops is its credit counting toward safe-to-spend.
/// </param>
/// <param name="BlockReason">"Lost", "Stolen", "Damaged" or "Compromised".</param>
public record CreditCardDto(
    Guid Id,
    string CardName,
    string BankName,
    decimal CreditLimit,
    decimal UsedCredit,
    decimal AvailableCredit,
    string Currency,
    decimal AnnualInterestRate,
    int PaymentDueDay,
    int StatementCutoffDay,
    string? LastFour,
    string TrackingMode,
    DateOnly? BlockedOn,
    string? BlockReason,
    DateOnly NextCutoffDate,
    DateOnly NextDueDate,
    int DaysUntilCutoff,
    int DaysUntilDue,
    DateOnly LastCutoffDate,
    decimal StatementBalance,
    decimal CurrentCycleCharges,
    decimal FutureInstallments,
    string? Notes,
    DateTime CreatedAt)
{
    /// <summary>
    /// Requires the split rather than computing it, because the division depends on
    /// purchase and installment history the card entity does not carry. Passing it
    /// in keeps every response — read or write — showing the same figures.
    /// </summary>
    /// <param name="today">
    /// The caller's own date, from IUserClock. Not DateTime.UtcNow: the cutoff and
    /// due dates below are "the next time this day of the month comes around", and
    /// computing that from a UTC date that has already rolled over answers next
    /// month on the very evening a card falls due.
    /// </param>
    public static CreditCardDto FromEntity(CreditCard card, StatementSplit split, DateOnly today)
    {
        var cutoff = IncomeMath.NextOccurrence(today, card.StatementCutoffDay);
        var due = LiquidityProjection.StatementDueDate(today, card.StatementCutoffDay, card.PaymentDueDay);

        return new(
            card.Id,
            card.CardName,
            card.BankName,
            card.CreditLimit.Amount,
            card.UsedCredit.Amount,
            card.AvailableCredit.Amount,
            card.CreditLimit.Currency,
            card.AnnualInterestRate,
            card.PaymentDueDay,
            card.StatementCutoffDay,
            card.LastFour,
            card.TrackingMode.ToString(),
            card.BlockedOn,
            card.BlockReason?.ToString(),
            cutoff,
            due,
            cutoff.DayNumber - today.DayNumber,
            due.DayNumber - today.DayNumber,
            split.LastCutoffDate,
            split.StatementBalance.Amount,
            split.CurrentCycleCharges.Amount,
            split.FutureInstallments.Amount,
            card.Notes,
            card.CreatedAt);
    }
}
