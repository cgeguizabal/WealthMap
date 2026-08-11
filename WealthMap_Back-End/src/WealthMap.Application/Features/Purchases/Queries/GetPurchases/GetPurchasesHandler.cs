using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Models;
using WealthMap.Application.Features.Purchases.DTOs;

namespace WealthMap.Application.Features.Purchases.Queries.GetPurchases;

public class GetPurchasesHandler : IQueryHandler<GetPurchasesQuery, PagedResult<PurchaseDto>>
{
    private readonly IPurchaseRepository _purchases;
    private readonly IStoreRepository _stores;

    public GetPurchasesHandler(IPurchaseRepository purchases, IStoreRepository stores)
    {
        _purchases = purchases;
        _stores = stores;
    }

    public async Task<PagedResult<PurchaseDto>> Handle(GetPurchasesQuery request, CancellationToken ct)
    {
        var totalCount = await _purchases.CountForUserAsync(
            request.UserId, request.Year, request.Month, request.Category, request.CreditCardId, ct);

        var items = await _purchases.GetPagedForUserAsync(
            request.UserId, request.Year, request.Month, request.Category, request.CreditCardId,
            request.Page, request.PageSize, ct);

        // One lookup for the page rather than a fetch per row. The catalogue is
        // small and shared, so this stays cheaper than joining per purchase.
        var storeNames = (await _stores.GetAllAsync(ct)).ToDictionary(s => s.Id, s => s.Name);

        return new PagedResult<PurchaseDto>(
            items.Select(p => PurchaseDto.FromEntity(p, NameOf(p.StoreId, storeNames))).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }

    /// <summary>A store removed from the catalogue leaves the purchase intact, so a
    /// missing name is expected rather than an error.</summary>
    private static string? NameOf(Guid? storeId, IReadOnlyDictionary<Guid, string> names) =>
        storeId is Guid id && names.TryGetValue(id, out var name) ? name : null;
}
