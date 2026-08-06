using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CreditCards.DTOs;

namespace WealthMap.Application.Features.CreditCards.Commands.UpdateCreditCard;

public record UpdateCreditCardCommand(
    Guid Id,
    Guid UserId,
    string CardName,
    string BankName,
    decimal AnnualInterestRate,
    int PaymentDueDay,
    int StatementCutoffDay,
    string? Notes) : ICommand<CreditCardDto>;