namespace WealthMap.Application.Common.Interfaces;

/// <summary>
/// Mints and hashes refresh token values. Kept behind an interface because the
/// Application layer must not reference System.Security.Cryptography directly —
/// and because "generate a random string" is exactly the kind of thing that gets
/// quietly replaced with something guessable if it is inlined at a call site.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>A new cryptographically random value. This is the only time it exists in full.</summary>
    string GenerateToken();

    /// <summary>
    /// SHA-256, hex-encoded. Deliberately not a password hash: this value is
    /// already high-entropy random, so a slow salted KDF would buy nothing and
    /// would make every request pay for it. It also has to be deterministic, since
    /// the hash is what the lookup matches on.
    /// </summary>
    string Hash(string token);

    /// <summary>How long a newly issued refresh token stays valid.</summary>
    TimeSpan Lifetime { get; }
}
