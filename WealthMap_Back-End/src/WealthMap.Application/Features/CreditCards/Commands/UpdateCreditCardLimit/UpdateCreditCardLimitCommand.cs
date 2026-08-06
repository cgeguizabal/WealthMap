using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CreditCards.DTOs;

namespace WealthMap.Application.Features.CreditCards.Commands.UpdateCreditCardLimit;

public record UpdateCreditCardLimitCommand(
    Guid Id,
    Guid UserId,
    decimal NewLimit) : ICommand<CreditCardDto>;