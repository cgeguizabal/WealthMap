using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class ProductGoalRepository : Repository<ProductGoal>, IProductGoalRepository
{
    public ProductGoalRepository(WealthMapDbContext context) : base(context) { }

    public async Task<ProductGoal?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId, ct);

    /// <remarks>Ordered in memory: `name` is encrypted, so the database cannot sort it.</remarks>
    public async Task<IReadOnlyList<ProductGoal>> GetAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var goals = await Set.Where(g => g.UserId == userId).AsNoTracking().ToListAsync(ct);

        return goals.OrderBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
    }
}
