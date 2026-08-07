using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Stores.DTOs;

namespace WealthMap.Application.Features.Stores.Commands.CreateStore;

public record CreateStoreCommand(
    Guid UserId,
    string Name,
    string Category,
    string? LogoUrl,
    string? Description) : ICommand<StoreDto>;