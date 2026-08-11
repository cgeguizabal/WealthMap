using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IPurchaseRepository : IRepository<Purchase>
{
    Task<Purchase?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<Purchase>> GetPagedForUserAsync(
        Guid userId, int? year, int? month, string? category, Guid? creditCardId,
        int page, int pageSize, CancellationToken ct = default);

    Task<int> CountForUserAsync(
        Guid userId, int? year, int? month, string? category, Guid? creditCardId,
        CancellationToken ct = default);

    Task<IReadOnlyList<Purchase>> GetForUserInMonthAsync(
        Guid userId, int year, int month, CancellationToken ct = default);
}
