using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Commands.BlockAccount;

public record BlockAccountCommand(Guid Id, Guid UserId) : ICommand<AccountDto>;