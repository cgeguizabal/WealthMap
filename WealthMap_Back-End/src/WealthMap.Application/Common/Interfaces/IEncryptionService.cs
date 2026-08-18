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

    /// <summary>
    /// The stamp every freshly encrypted value carries, e.g. <c>"v2:"</c>.
    /// </summary>
    /// <remarks>
    /// Exposed so the re-encryption pass can ask which rows are behind the
    /// current key rather than hard-coding a generation. That one question is
    /// what lets the same pass do the first encryption and every later rotation.
    /// </remarks>
    string CurrentVersionPrefix { get; }

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

    /// <summary>
    /// Every blind index that could match this value — the current key's, and
    /// the previous key's during a rotation.
    /// </summary>
    /// <remarks>
    /// A blind index is a bare hash with no version stamp, so it cannot say which
    /// key produced it. While a rotation is in flight the table holds both, and a
    /// lookup that only tried the current one would report half the users as not
    /// existing. Returns one value at rest.
    /// </remarks>
    IReadOnlyList<string> BlindIndexCandidates(string value);
}