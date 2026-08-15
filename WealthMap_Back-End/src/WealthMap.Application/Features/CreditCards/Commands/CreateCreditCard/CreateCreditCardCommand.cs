using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CreditCards.DTOs;

namespace WealthMap.Application.Features.CreditCards.Commands.CreateCreditCard;

/// <param name="LastFour">Optional at creation, so identifying a card is not a second step.</param>
/// <param name="TrackingMode">Optional; defaults to Manual when omitted.</param>
public record CreateCreditCardCommand(
    Guid UserId,
    string CardName,
    string BankName,
    decimal CreditLimit,
    string Currency,
    decimal AnnualInterestRate,
    int PaymentDueDay,
    int StatementCutoffDay,
    string? LastFour = null,
    int? TrackingMode = null) : ICommand<CreditCardDto>;
