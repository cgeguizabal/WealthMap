using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Models;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Queries.GetAccountMovements;

public record GetAccountMovementsQuery(
    Guid AccountId,
    Guid UserId,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<AccountMovementDto>>;