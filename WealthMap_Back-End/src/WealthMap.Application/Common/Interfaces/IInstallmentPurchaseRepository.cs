using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IInstallmentPurchaseRepository : IRepository<InstallmentPurchase>
{
    Task<InstallmentPurchase?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<InstallmentPurchase>> GetAllForUserAsync(Guid userId, CancellationToken ct = default);
}
