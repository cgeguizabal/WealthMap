using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class DebtRepository : Repository<Debt>, IDebtRepository
{
    public DebtRepository(WealthMapDbContext context) : base(context) { }

    public async Task<Debt?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId, ct);

    /// <remarks>Ordered in memory: `name` is encrypted, so the database cannot sort it.</remarks>
    public async Task<IReadOnlyList<Debt>> GetAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var debts = await Set.Where(d => d.UserId == userId).AsNoTracking().ToListAsync(ct);

        return debts.OrderBy(d => d.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
    }
}
