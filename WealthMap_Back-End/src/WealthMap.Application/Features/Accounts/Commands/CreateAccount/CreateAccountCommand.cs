using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Commands.CreateAccount;

public record CreateAccountCommand(
    Guid UserId,
    string Name,
    string BankName,
    int Type,
    decimal OpeningBalance,
    string Currency) : ICommand<AccountDto>;