using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IPurchaseRepository : IRepository<Purchase>
{
    Task<Purchase?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<Purchase>> GetPagedForUserAsync(
        Guid userId, int? year, int? month, string? category,
        int page, int pageSize, CancellationToken ct = default);

    Task<int> CountForUserAsync(
        Guid userId, int? year, int? month, string? category,
        CancellationToken ct = default);
}
