using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(WealthMapDbContext context) : base(context) { }

    /// <summary>
    /// Returns revoked and expired tokens too. The caller has to tell those cases
    /// apart — a revoked token being presented is a replay and triggers a full
    /// revocation, so filtering it out here would hide the one signal that matters.
    /// </summary>
    public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(
        Guid userId, CancellationToken ct = default) =>
        await Set
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);

    public async Task<int> DeleteExpiredAsync(DateTime olderThan, CancellationToken ct = default) =>
        await Set.Where(t => t.ExpiresAt < olderThan).ExecuteDeleteAsync(ct);
}
