using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Commands.DepositToAccount;

public record DepositToAccountCommand(
    Guid AccountId,
    Guid UserId,
    decimal Amount,
    string Description,
    int Type) : ICommand<AccountMovementDto>;