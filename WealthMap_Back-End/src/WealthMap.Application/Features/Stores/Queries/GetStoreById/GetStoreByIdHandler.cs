using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Stores.DTOs;

namespace WealthMap.Application.Features.Stores.Queries.GetStoreById;

public class GetStoreByIdHandler : IQueryHandler<GetStoreByIdQuery, StoreDto>
{
    private readonly IStoreRepository _stores;

    public GetStoreByIdHandler(IStoreRepository stores) => _stores = stores;

    public async Task<StoreDto> Handle(GetStoreByIdQuery request, CancellationToken ct)
    {
        var store = await _stores.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Store", request.Id);

        return StoreDto.FromEntity(store, request.UserId);
    }
}