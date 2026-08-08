using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class AccountMovementRepository : Repository<AccountMovement>, IAccountMovementRepository
{
    public AccountMovementRepository(WealthMapDbContext context) : base(context) { }

    public async Task<IReadOnlyList<AccountMovement>> GetPagedForAccountAsync(
        Guid accountId, Guid userId, int page, int pageSize, CancellationToken ct = default) =>
        await Set.Where(m => m.AccountId == accountId && m.UserId == userId)
                 .OrderByDescending(m => m.OccurredAt)
                 .ThenByDescending(m => m.CreatedAt)
                 .Skip((page - 1) * pageSize)
                 .Take(pageSize)
                 .AsNoTracking()
                 .ToListAsync(ct);

    public async Task<int> CountForAccountAsync(Guid accountId, Guid userId, CancellationToken ct = default) =>
        await Set.CountAsync(m => m.AccountId == accountId && m.UserId == userId, ct);

    public async Task<IReadOnlyList<AccountMovement>> GetForUserFromAsync(
        Guid userId, DateTime fromInclusive, CancellationToken ct = default) =>
        await Set.Where(m => m.UserId == userId && m.OccurredAt >= fromInclusive)
                 .OrderBy(m => m.OccurredAt)
                 .AsNoTracking()
                 .ToListAsync(ct);
}