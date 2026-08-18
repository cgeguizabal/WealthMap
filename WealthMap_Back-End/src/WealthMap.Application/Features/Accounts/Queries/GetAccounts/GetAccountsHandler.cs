using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Queries.GetAccounts;

public class GetAccountsHandler : IQueryHandler<GetAccountsQuery, IReadOnlyList<AccountDto>>
{
    private readonly IAccountRepository _accounts;

    public GetAccountsHandler(IAccountRepository accounts) => _accounts = accounts;

    public async Task<IReadOnlyList<AccountDto>> Handle(GetAccountsQuery request, CancellationToken ct)
    {
        var accounts = await _accounts.GetAllForUserAsync(
            request.UserId, request.IncludeArchived, ct);

        return accounts.Select(AccountDto.FromEntity).ToList();
    }
}