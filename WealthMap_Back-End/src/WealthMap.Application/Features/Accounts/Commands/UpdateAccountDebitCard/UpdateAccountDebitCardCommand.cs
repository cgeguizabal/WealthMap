using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Commands.UpdateAccountDebitCard;

/// <summary>
/// Sets both together, because the type governs the digits: choosing None clears
/// them rather than leaving a number for a card that does not exist.
/// </summary>
public record UpdateAccountDebitCardCommand(
    Guid Id,
    Guid UserId,
    int DebitCardType,
    string? DebitCardLastFour) : ICommand<AccountDto>;
