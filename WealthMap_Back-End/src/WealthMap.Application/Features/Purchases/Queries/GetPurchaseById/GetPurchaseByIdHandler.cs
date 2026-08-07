using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Purchases.DTOs;

namespace WealthMap.Application.Features.Purchases.Queries.GetPurchaseById;

public class GetPurchaseByIdHandler : IQueryHandler<GetPurchaseByIdQuery, PurchaseDto>
{
    private readonly IPurchaseRepository _purchases;

    public GetPurchaseByIdHandler(IPurchaseRepository purchases) => _purchases = purchases;

    public async Task<PurchaseDto> Handle(GetPurchaseByIdQuery request, CancellationToken ct)
    {
        var purchase = await _purchases.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Purchase", request.Id);

        return PurchaseDto.FromEntity(purchase);
    }
}
