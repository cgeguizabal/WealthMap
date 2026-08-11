using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Models;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Queries.GetAccountMovements;

public class GetAccountMovementsHandler
    : IQueryHandler<GetAccountMovementsQuery, PagedResult<AccountMovementDto>>
{
    private readonly IAccountRepository _accounts;
    private readonly IAccountMovementRepository _movements;

    public GetAccountMovementsHandler(IAccountRepository accounts, IAccountMovementRepository movements)
    {
        _accounts = accounts;
        _movements = movements;
    }

    public async Task<PagedResult<AccountMovementDto>> Handle(GetAccountMovementsQuery request, CancellationToken ct)
    {
        // Fetched by id rather than ExistsForUserAsync: that check excludes archived
        // accounts, and archiving one must not take its movement history with it.
        if (await _accounts.GetByIdForUserAsync(request.AccountId, request.UserId, ct) is null)
            throw new NotFoundException("Account", request.AccountId);

        var totalCount = await _movements.CountForAccountAsync(request.AccountId, request.UserId, ct);

        var items = await _movements.GetPagedForAccountAsync(
            request.AccountId, request.UserId, request.Page, request.PageSize, ct);

        return new PagedResult<AccountMovementDto>(
            items.Select(AccountMovementDto.FromEntity).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }
}