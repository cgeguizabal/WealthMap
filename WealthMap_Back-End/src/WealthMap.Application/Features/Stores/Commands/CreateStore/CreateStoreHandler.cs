using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Stores.DTOs;
using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.Stores.Commands.CreateStore;

public class CreateStoreHandler : ICommandHandler<CreateStoreCommand, StoreDto>
{
    private readonly IStoreRepository _stores;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStoreHandler(IStoreRepository stores, IUnitOfWork unitOfWork)
    {
        _stores = stores;
        _unitOfWork = unitOfWork;
    }

    public async Task<StoreDto> Handle(CreateStoreCommand request, CancellationToken ct)
    {
        var store = new Store(
            request.UserId,
            request.Name,
            request.Category,
            request.LogoUrl,
            request.Description);

        await _stores.AddAsync(store, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return StoreDto.FromEntity(store, request.UserId);
    }
}