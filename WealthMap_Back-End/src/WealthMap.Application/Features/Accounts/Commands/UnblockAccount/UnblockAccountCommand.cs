using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Commands.UnblockAccount;

public record UnblockAccountCommand(Guid Id, Guid UserId) : ICommand<AccountDto>;