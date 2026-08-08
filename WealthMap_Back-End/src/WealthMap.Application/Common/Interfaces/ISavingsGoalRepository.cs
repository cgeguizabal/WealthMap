using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface ISavingsGoalRepository : IRepository<SavingsGoal>
{
    Task<SavingsGoal?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<SavingsGoal>> GetAllForUserAsync(Guid userId, CancellationToken ct = default);
}
