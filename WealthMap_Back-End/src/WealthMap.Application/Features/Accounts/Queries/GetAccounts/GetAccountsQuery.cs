using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Queries.GetAccounts;

/// <param name="IncludeArchived">
/// Archived accounts are hidden from every list and total by default. The
/// settings screen asks for them so they can be brought back — without that,
/// archiving is a one-way door.
/// </param>
public record GetAccountsQuery(
    Guid UserId,
    bool IncludeArchived = false) : IQuery<IReadOnlyList<AccountDto>>;