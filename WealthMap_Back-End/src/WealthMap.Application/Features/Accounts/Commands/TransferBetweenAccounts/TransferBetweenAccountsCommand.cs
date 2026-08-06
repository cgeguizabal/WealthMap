using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Commands.TransferBetweenAccounts;

public record TransferBetweenAccountsCommand(
    Guid UserId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string? Description) : ICommand<TransferResultDto>;