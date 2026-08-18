using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>
    /// The one query in the application that is not user-scoped, and cannot be:
    /// the caller presents a token and nothing else, so the hash *is* the claim of
    /// identity. Whoever holds the matching value is the user — which is exactly
    /// why the raw value is never stored and rotation is mandatory.
    /// </summary>
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>
    /// Every unrevoked token for a user. Used to end all sessions at once when a
    /// revoked token is presented again, which means a copy is circulating.
    /// </summary>
    Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Drops rows that are long dead, so the table does not grow without bound.</summary>
    Task<int> DeleteExpiredAsync(DateTime olderThan, CancellationToken ct = default);
}
