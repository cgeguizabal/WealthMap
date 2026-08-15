using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.Accounts.DTOs;

/// <param name="LastFour">Last four of the account number, or null.</param>
/// <param name="TrackingMode">"Manual" or "EmailSync". Only Manual does anything today.</param>
/// <param name="DebitCardType">"None", "Physical" or "Digital".</param>
/// <param name="DebitCardLastFour">
/// Last four of the debit card — a different number from the account's own, and
/// always null when there is no card.
/// </param>
public record AccountDto(
    Guid Id,
    string Name,
    string BankName,
    string Type,
    decimal Balance,
    string Currency,
    bool IsBlockedForSaving,
    string? LastFour,
    string TrackingMode,
    string DebitCardType,
    string? DebitCardLastFour,
    string? Notes,
    DateTime CreatedAt)
{
    public static AccountDto FromEntity(Account account) => new(
        account.Id,
        account.Name,
        account.BankName,
        account.Type.ToString(),
        account.Balance.Amount,
        account.Balance.Currency,
        account.IsBlockedForSaving,
        account.LastFour,
        account.TrackingMode.ToString(),
        account.DebitCardType.ToString(),
        account.DebitCardLastFour,
        account.Notes,
        account.CreatedAt);
}
