using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Commands.WithdrawFromAccount;

public record WithdrawFromAccountCommand(
    Guid AccountId,
    Guid UserId,
    decimal Amount,
    string Description,
    string? Location) : ICommand<AccountMovementDto>;