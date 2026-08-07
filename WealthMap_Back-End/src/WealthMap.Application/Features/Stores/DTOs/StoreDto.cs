using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.Stores.DTOs;

public record StoreDto(
    Guid Id,
    string Name,
    string Category,
    string? LogoUrl,
    string? Description,
    bool IsMine,
    DateTime CreatedAt)
{
    public static StoreDto FromEntity(Store store, Guid currentUserId) => new(
        store.Id,
        store.Name,
        store.Category,
        store.LogoUrl,
        store.Description,
        store.IsOwnedBy(currentUserId),
        store.CreatedAt);
}