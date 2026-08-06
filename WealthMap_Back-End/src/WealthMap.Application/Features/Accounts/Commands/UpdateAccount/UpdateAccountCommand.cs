using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Commands.UpdateAccount;

public record UpdateAccountCommand(
    Guid Id,
    Guid UserId,
    string Name,
    string BankName,
    string? Notes) : ICommand<AccountDto>;