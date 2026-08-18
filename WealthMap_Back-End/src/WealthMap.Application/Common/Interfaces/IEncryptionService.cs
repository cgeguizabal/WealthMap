namespace WealthMap.Application.Common.Interfaces;

/// <summary>
/// Encrypts the columns that identify a person, so a database dump yields
/// financial activity that cannot be attributed to anyone.
/// </summary>
/// <remarks>
/// This is pseudonymisation, not zero-knowledge. The server holds the key,
/// because the dashboard, the alerts and the monthly report all compute on
/// plaintext. It defends against a leaked dump, a stolen backup and idle
/// browsing by whoever has database access — not against the operator.
/// </remarks>
public interface IEncryptionService
{
    /// <summary>Randomised: the same input encrypts differently every time.</summary>
    string Encrypt(string plaintext);

    /// <summary>Returns anything without a recognised version prefix unchanged.</summary>
    string Decrypt(string ciphertext);

    /// <summary>
    /// Deterministic and one-way, so equality search survives encryption.
    /// </summary>
    /// <remarks>
    /// Randomised ciphertext cannot be looked up or made unique — two encryptions
    /// of one email differ. A blind index is a keyed hash of the normalised value:
    /// the same input always yields the same output, so it can carry a unique
    /// index and answer "does this email exist", while revealing nothing to anyone
    /// without the key.
    /// </remarks>
    string BlindIndex(string value);
}
