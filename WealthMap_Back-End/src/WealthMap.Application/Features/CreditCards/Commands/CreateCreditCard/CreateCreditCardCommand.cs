using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CreditCards.DTOs;

namespace WealthMap.Application.Features.CreditCards.Commands.CreateCreditCard;

public record CreateCreditCardCommand(
    Guid UserId,
    string CardName,
    string BankName,
    decimal CreditLimit,
    string Currency,
    decimal AnnualInterestRate,
    int PaymentDueDay,
    int StatementCutoffDay) : ICommand<CreditCardDto>;