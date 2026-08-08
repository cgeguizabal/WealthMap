using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class DebtRepository : Repository<Debt>, IDebtRepository
{
    public DebtRepository(WealthMapDbContext context) : base(context) { }

    public async Task<Debt?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId, ct);

    public async Task<IReadOnlyList<Debt>> GetAllForUserAsync(Guid userId, CancellationToken ct = default) =>
        await Set.Where(d => d.UserId == userId)
                 .OrderBy(d => d.Name)
                 .AsNoTracking()
                 .ToListAsync(ct);
}
