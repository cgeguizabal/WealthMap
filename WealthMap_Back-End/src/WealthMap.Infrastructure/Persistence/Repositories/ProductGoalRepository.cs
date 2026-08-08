using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class ProductGoalRepository : Repository<ProductGoal>, IProductGoalRepository
{
    public ProductGoalRepository(WealthMapDbContext context) : base(context) { }

    public async Task<ProductGoal?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId, ct);

    public async Task<IReadOnlyList<ProductGoal>> GetAllForUserAsync(Guid userId, CancellationToken ct = default) =>
        await Set.Where(g => g.UserId == userId)
                 .OrderBy(g => g.Name)
                 .AsNoTracking()
                 .ToListAsync(ct);
}
