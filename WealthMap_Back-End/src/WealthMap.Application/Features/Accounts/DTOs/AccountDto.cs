using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.Accounts.DTOs;

public record AccountDto(
    Guid Id,
    string Name,
    string BankName,
    string Type,
    decimal Balance,
    string Currency,
    bool IsBlockedForSaving,
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
        account.Notes,
        account.CreatedAt);
}