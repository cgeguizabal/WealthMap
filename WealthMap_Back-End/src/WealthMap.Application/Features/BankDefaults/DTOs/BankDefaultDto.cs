using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.BankDefaults.DTOs;

/// <param name="DefaultAccountName">
/// Resolved for display. Without it every client would have to fetch the account
/// list purely to render one row, and a table of bare GUIDs is unreadable.
/// </param>
public record BankDefaultDto(
    Guid Id,
    string BankName,
    string Direction,
    Guid DefaultAccountId,
    string DefaultAccountName,
    DateTime CreatedAt)
{
    public static BankDefaultDto FromEntity(BankDefault bankDefault, string accountName) => new(
        bankDefault.Id,
        bankDefault.BankName,
        bankDefault.Direction.ToString(),
        bankDefault.DefaultAccountId,
        accountName,
        bankDefault.CreatedAt);
}
