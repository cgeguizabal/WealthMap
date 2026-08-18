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

    public async Task<AccountMovement?> GetByRelatedEntityAsync(
        Guid relatedEntityId, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(
            m => m.RelatedEntityId == relatedEntityId && m.UserId == userId, ct);

    // Tracked, not AsNoTracking: these are loaded in order to be rebased and saved.
    public async Task<IReadOnlyList<AccountMovement>> GetForAccountAfterAsync(
        Guid accountId, Guid userId, DateTime after, CancellationToken ct = default) =>
        await Set.Where(m => m.AccountId == accountId && m.UserId == userId && m.OccurredAt > after)
                 .OrderBy(m => m.OccurredAt)
                 .ToListAsync(ct);
}