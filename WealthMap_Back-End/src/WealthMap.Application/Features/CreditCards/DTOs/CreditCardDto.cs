using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.CreditCards.DTOs;

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
    string? Notes,
    DateTime CreatedAt)
{
    public static CreditCardDto FromEntity(CreditCard card) => new(
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
        card.Notes,
        card.CreatedAt);
}