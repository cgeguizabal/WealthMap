using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Commands.CreateAccount;

/// <param name="LastFour">Optional at creation, so identifying an account is not a second step.</param>
/// <param name="TrackingMode">Optional; defaults to Manual when omitted.</param>
public record CreateAccountCommand(
    Guid UserId,
    string Name,
    string BankName,
    int Type,
    decimal OpeningBalance,
    string Currency,
    string? LastFour = null,
    int? TrackingMode = null,
    int? DebitCardType = null,
    string? DebitCardLastFour = null) : ICommand<AccountDto>;
