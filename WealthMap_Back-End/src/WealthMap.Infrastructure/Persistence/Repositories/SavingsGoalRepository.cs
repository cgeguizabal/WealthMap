using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class SavingsGoalRepository : Repository<SavingsGoal>, ISavingsGoalRepository
{
    public SavingsGoalRepository(WealthMapDbContext context) : base(context) { }

    public async Task<SavingsGoal?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId, ct);

    public async Task<IReadOnlyList<SavingsGoal>> GetAllForUserAsync(Guid userId, CancellationToken ct = default) =>
        await Set.Where(g => g.UserId == userId)
                 .OrderBy(g => g.Deadline)
                 .AsNoTracking()
                 .ToListAsync(ct);
}
