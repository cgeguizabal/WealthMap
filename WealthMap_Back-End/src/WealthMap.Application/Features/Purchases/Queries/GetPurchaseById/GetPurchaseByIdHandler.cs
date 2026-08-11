using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Purchases.DTOs;

namespace WealthMap.Application.Features.Purchases.Queries.GetPurchaseById;

public class GetPurchaseByIdHandler : IQueryHandler<GetPurchaseByIdQuery, PurchaseDto>
{
    private readonly IPurchaseRepository _purchases;
    private readonly IStoreRepository _stores;

    public GetPurchaseByIdHandler(IPurchaseRepository purchases, IStoreRepository stores)
    {
        _purchases = purchases;
        _stores = stores;
    }

    public async Task<PurchaseDto> Handle(GetPurchaseByIdQuery request, CancellationToken ct)
    {
        var purchase = await _purchases.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Purchase", request.Id);

        // A store removed from the catalogue leaves the purchase intact, so this
        // stays null rather than failing the read.
        var storeName = purchase.StoreId is Guid storeId
            ? (await _stores.GetByIdAsync(storeId, ct))?.Name
            : null;

        return PurchaseDto.FromEntity(purchase, storeName);
    }
}
