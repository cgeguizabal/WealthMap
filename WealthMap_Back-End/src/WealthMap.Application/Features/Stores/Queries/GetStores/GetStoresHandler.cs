using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Stores.DTOs;

namespace WealthMap.Application.Features.Stores.Queries.GetStores;

public class GetStoresHandler : IQueryHandler<GetStoresQuery, IReadOnlyList<StoreDto>>
{
    private readonly IStoreRepository _stores;

    public GetStoresHandler(IStoreRepository stores) => _stores = stores;

    public async Task<IReadOnlyList<StoreDto>> Handle(GetStoresQuery request, CancellationToken ct)
    {
        var stores = await _stores.GetAllAsync(ct);
        return stores.Select(s => StoreDto.FromEntity(s, request.UserId)).ToList();
    }
}