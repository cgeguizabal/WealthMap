using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(WealthMapDbContext context) : base(context) { }

    /// <summary>Archived accounts are still fetchable by id, so their detail page keeps working.</summary>
    public async Task<Account?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);

    public async Task<IReadOnlyList<Account>> GetAllForUserAsync(
        Guid userId, bool includeArchived = false, CancellationToken ct = default)
    {
        var query = Set.Where(a => a.UserId == userId);

        if (!includeArchived)
            query = query.Where(a => !a.IsArchived);

        return await query
            .OrderBy(a => a.Name)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.AnyAsync(a => a.Id == id && a.UserId == userId && !a.IsArchived, ct);
}
