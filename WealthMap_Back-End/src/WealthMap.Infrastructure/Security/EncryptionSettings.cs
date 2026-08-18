namespace WealthMap.Infrastructure.Security;

/// <summary>
/// The two keys, base64-encoded, 32 bytes each once decoded.
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
}
