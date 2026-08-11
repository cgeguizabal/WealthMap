using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Models;
using WealthMap.Application.Features.Purchases.DTOs;

namespace WealthMap.Application.Features.Purchases.Queries.GetPurchases;

public record GetPurchasesQuery(
    Guid UserId,
    int? Year = null,
    int? Month = null,
    string? Category = null,
    /// <summary>Narrows to purchases charged to one card. Ignored when null.</summary>
    Guid? CreditCardId = null,
    int Page = 1,
    int PageSize = PagedQueryRules.DefaultPageSize)
    : IQuery<PagedResult<PurchaseDto>>, IPagedQuery;
