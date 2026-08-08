using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IProductGoalRepository : IRepository<ProductGoal>
{
    Task<ProductGoal?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductGoal>> GetAllForUserAsync(Guid userId, CancellationToken ct = default);
}
