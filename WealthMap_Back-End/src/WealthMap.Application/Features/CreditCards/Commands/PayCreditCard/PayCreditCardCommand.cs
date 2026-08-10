using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CreditCards.DTOs;

namespace WealthMap.Application.Features.CreditCards.Commands.PayCreditCard;

public record PayCreditCardCommand(
    Guid CardId,
    Guid UserId,
    decimal Amount,
    string SourceType,
    Guid? SourceAccountId,
    string? Notes = null) : ICommand<CardPaymentResultDto>;