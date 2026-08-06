using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Queries.GetAccounts;

public record GetAccountsQuery(Guid UserId) : IQuery<IReadOnlyList<AccountDto>>;