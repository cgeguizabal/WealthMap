using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.Accounts.DTOs;

/// <param name="LastFour">The digits a bank prints when naming this account, or null.</param>
/// <param name="TrackingMode">"Manual" or "EmailSync". Only Manual does anything today.</param>
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
        account.Notes,
        account.CreatedAt);
}
