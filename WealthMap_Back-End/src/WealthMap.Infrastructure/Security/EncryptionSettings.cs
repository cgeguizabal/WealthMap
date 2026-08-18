namespace WealthMap.Infrastructure.Security;

/// <summary>
/// The encryption keys, base64-encoded, 32 bytes each once decoded.
/// </summary>
/// <remarks>
/// Kept out of the database on purpose: a dump that carried its own key would
/// defend against nothing. In development they come from user-secrets, the same
/// place the connection string lives; in production from the host's secret store.
/// </remarks>
public class EncryptionSettings
{
    public const string SectionName = "Encryption";

    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// Separate from <see cref="Key"/> deliberately. If one leaks, the other still
    /// holds: the encryption key alone cannot reproduce a blind index, and the
    /// blind-index key alone decrypts nothing.
    /// </summary>
    public string BlindIndexKey { get; init; } = string.Empty;

    /// <summary>
    /// Which generation of <see cref="Key"/> is in use. Stamped into every value
    /// written, as the <c>v2:</c> in <c>v2:{base64}</c>.
    /// </summary>
    /// <remarks>
    /// Raise it by one when rotating, and put the outgoing key in
    /// <see cref="PreviousKey"/>. Rows written before the change keep their old
    /// stamp and are still readable, which is what makes rotation something that
    /// can happen while the app is serving traffic rather than during an outage.
    /// </remarks>
    public int KeyVersion { get; init; } = 1;

    /// <summary>
    /// The key that wrote generation <see cref="KeyVersion"/> − 1, during a
    /// rotation. Empty at rest.
    /// </summary>
    /// <remarks>
    /// Only decrypts; nothing is ever written with it. Remove it once the
    /// re-encryption pass reports no rows left on the old generation — leaving it
    /// in place indefinitely means a leaked key stays useful.
    /// </remarks>
    public string PreviousKey { get; init; } = string.Empty;

    /// <summary>
    /// The blind-index key that was in use before the current one.
    /// </summary>
    /// <remarks>
    /// A blind index carries no version stamp — it is a bare hash, and cannot say
    /// which key made it. So sign-in tries the current key and then this one,
    /// which is what keeps people able to log in while the re-encryption pass
    /// works through the table. Remove it when the pass is done.
    /// </remarks>
    public string PreviousBlindIndexKey { get; init; } = string.Empty;
}
