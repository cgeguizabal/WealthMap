using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Models;
using WealthMap.Application.Features.Purchases.DTOs;

namespace WealthMap.Application.Features.Purchases.Queries.GetPurchases;

public record GetPurchasesQuery(
    Guid UserId,
    int? Year = null,
    int? Month = null,
    string? Category = null,
    int Page = 1,
    int PageSize = PagedQueryRules.DefaultPageSize)
    : IQuery<PagedResult<PurchaseDto>>, IPagedQuery;
