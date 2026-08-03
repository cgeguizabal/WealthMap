using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(WealthMapDbContext context) : base(context) { }

    public async Task<Account?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);

    public async Task<IReadOnlyList<Account>> GetAllForUserAsync(Guid userId, CancellationToken ct = default) =>
        await Set.Where(a => a.UserId == userId)
                 .OrderBy(a => a.Name)
                 .AsNoTracking()
                 .ToListAsync(ct);

    public async Task<bool> ExistsForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.AnyAsync(a => a.Id == id && a.UserId == userId, ct);
}