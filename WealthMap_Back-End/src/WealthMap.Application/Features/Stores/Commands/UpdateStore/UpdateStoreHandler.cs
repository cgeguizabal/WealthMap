using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Stores.DTOs;

namespace WealthMap.Application.Features.Stores.Commands.UpdateStore;

public class UpdateStoreHandler : ICommandHandler<UpdateStoreCommand, StoreDto>
{
    private readonly IStoreRepository _stores;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStoreHandler(IStoreRepository stores, IUnitOfWork unitOfWork)
    {
        _stores = stores;
        _unitOfWork = unitOfWork;
    }

    public async Task<StoreDto> Handle(UpdateStoreCommand request, CancellationToken ct)
    {
        var store = await _stores.GetByIdAsync(request.Id, ct);

        // Editing someone else's store answers 404, same as "not yours" everywhere:
        // the catalog is shared to read, not to write.
        if (store is null || !store.IsOwnedBy(request.UserId))
            throw new NotFoundException("Store", request.Id);

        store.UpdateDetails(request.Name, request.Category, request.LogoUrl, request.Description);

        await _unitOfWork.SaveChangesAsync(ct);

        return StoreDto.FromEntity(store, request.UserId);
    }
}