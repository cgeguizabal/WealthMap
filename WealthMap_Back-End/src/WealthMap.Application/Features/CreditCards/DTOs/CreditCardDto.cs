using WealthMap.Domain.Entities;
using WealthMap.Domain.Services;

namespace WealthMap.Application.Features.CreditCards.DTOs;

/// <param name="NextCutoffDate">When the current statement closes.</param>
/// <param name="NextDueDate">
/// When today's balance has to be paid. Not simply the next occurrence of
/// <paramref name="PaymentDueDay"/>: spending is billed on the statement closing at
/// the next cutoff, and that statement is due on the following due day. This is the
/// same date the dashboard's safe-to-spend projection reserves against, so the two
/// screens cannot tell the user different things.
/// </param>
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
    DateOnly NextCutoffDate,
    DateOnly NextDueDate,
    int DaysUntilCutoff,
    int DaysUntilDue,
    string? Notes,
    DateTime CreatedAt)
{
    public static CreditCardDto FromEntity(CreditCard card)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
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
            cutoff,
            due,
            cutoff.DayNumber - today.DayNumber,
            due.DayNumber - today.DayNumber,
            card.Notes,
            card.CreatedAt);
    }
}