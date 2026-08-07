using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Stores.DTOs;

namespace WealthMap.Application.Features.Stores.Commands.UpdateStore;

public record UpdateStoreCommand(
    Guid Id,
    Guid UserId,
    string Name,
    string Category,
    string? LogoUrl,
    string? Description) : ICommand<StoreDto>;