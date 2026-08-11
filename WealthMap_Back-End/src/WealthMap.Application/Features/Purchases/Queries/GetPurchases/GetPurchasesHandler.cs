using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Models;
using WealthMap.Application.Features.Purchases.DTOs;

namespace WealthMap.Application.Features.Purchases.Queries.GetPurchases;

public class GetPurchasesHandler : IQueryHandler<GetPurchasesQuery, PagedResult<PurchaseDto>>
{
    private readonly IPurchaseRepository _purchases;

    public GetPurchasesHandler(IPurchaseRepository purchases) => _purchases = purchases;

    public async Task<PagedResult<PurchaseDto>> Handle(GetPurchasesQuery request, CancellationToken ct)
    {
        var totalCount = await _purchases.CountForUserAsync(
            request.UserId, request.Year, request.Month, request.Category, request.CreditCardId, ct);

        var items = await _purchases.GetPagedForUserAsync(
            request.UserId, request.Year, request.Month, request.Category, request.CreditCardId,
            request.Page, request.PageSize, ct);

        return new PagedResult<PurchaseDto>(
            items.Select(PurchaseDto.FromEntity).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }
}
