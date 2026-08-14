using WealthMap.Domain.Common;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Domain.Entities;

/// <summary>
/// One issued refresh token. Only ever holds the SHA-256 hash of the value the
/// client was given, never the value itself.
/// </summary>
/// <remarks>
/// Hashing matters more here than for a password. A refresh token is a bearer
/// credential with a two-week life, so a leaked database table would otherwise be
/// a set of working logins. Hashed, the rows are useless without the originals.
///
/// Rotation is the other half: refreshing revokes the token that was presented
/// and records which one replaced it. That chain is what makes theft detectable
/// — see <see cref="Revoke"/>'s note on presenting an already-revoked token.
/// </remarks>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    /// <summary>Set when this token was rotated, so a reuse can be traced forward.</summary>
    public string? ReplacedByTokenHash { get; private set; }

    private RefreshToken()
    {
        TokenHash = null!;
    } // required by EF Core

    public RefreshToken(Guid userId, string tokenHash, DateTime expiresAt)
    {
        if (userId == Guid.Empty)
            throw new DomainException("A refresh token must belong to a user.");

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException("Token hash is required.");

        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("A refresh token cannot expire in the past.");

        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>Usable exactly once, and only while unrevoked and unexpired.</summary>
    public bool IsActive => !IsRevoked && !IsExpired;

    /// <summary>
    /// Revoking is idempotent on purpose. Logout can be called twice, and the
    /// second call should not fail — unlike archiving, there is nothing for the
    /// user to correct.
    /// </summary>
    public void Revoke()
    {
        if (IsRevoked) return;

        RevokedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Rotation: this token is spent, and <paramref name="newTokenHash"/> takes over.</summary>
    public void ReplaceWith(string newTokenHash)
    {
        if (string.IsNullOrWhiteSpace(newTokenHash))
            throw new DomainException("Replacement token hash is required.");

        if (IsRevoked)
            throw new DomainException("This refresh token has already been used.");

        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenHash = newTokenHash;
        Touch();
    }
}
