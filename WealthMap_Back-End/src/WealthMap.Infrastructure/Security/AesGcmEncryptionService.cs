using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using WealthMap.Application.Common.Interfaces;

namespace WealthMap.Infrastructure.Security;

/// <summary>
/// AES-256-GCM for the identifying columns, HMAC-SHA256 for their blind indexes.
/// </summary>
/// <remarks>
/// GCM rather than CBC because it is authenticated: a tampered ciphertext fails
/// to decrypt loudly instead of yielding plausible garbage that then flows into
/// a report. A row edited directly in the database is detected, not displayed.
///
/// Stateless, so registered as a singleton. The keys are read once at startup and
/// the class holds no per-request state.
/// </remarks>
public class AesGcmEncryptionService : IEncryptionService
{
    /// <summary>
    /// Marks the algorithm and key generation that produced a value.
    /// </summary>
    /// <remarks>
    /// Mandatory, and worth the four bytes. Without it, rotating a key later means
    /// guessing which rows are old — with it, a future v2 can decrypt v1 values on
    /// read and rewrite them, and <see cref="Decrypt"/> can recognise plaintext
    /// that predates encryption entirely.
    /// </remarks>
    private const string Version = "v1:";

    // GCM's standard nonce size. Twelve bytes is what the construction is defined
    // for; other lengths force an extra hashing step and weaken the security proof.
    private const int NonceBytes = 12;
    private const int TagBytes = 16;
    private const int KeyBytes = 32;

    private readonly byte[] _key;
    private readonly byte[] _blindIndexKey;

    public AesGcmEncryptionService(IOptions<EncryptionSettings> options)
    {
        var settings = options.Value;

        _key = ReadKey(settings.Key, $"{EncryptionSettings.SectionName}:Key");
        _blindIndexKey = ReadKey(
            settings.BlindIndexKey, $"{EncryptionSettings.SectionName}:BlindIndexKey");
    }

    /// <summary>
    /// Fails at startup rather than at the first request that needs a key.
    /// </summary>
    /// <remarks>
    /// The same reasoning as the connection-string check in AddInfrastructure: an
    /// app that boots without a key would run until someone registered, then throw
    /// on a path nobody was watching. Refusing to start is the louder failure.
    /// </remarks>
    private static byte[] ReadKey(string configured, string path)
    {
        if (string.IsNullOrWhiteSpace(configured))
            throw new InvalidOperationException(
                $"'{path}' is not configured. Generate one with: " +
                "$b=[byte[]]::new(32); " +
                "[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); " +
                "[Convert]::ToBase64String($b)");

        byte[] key;

        try
        {
            key = Convert.FromBase64String(configured);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException($"'{path}' is not valid base64.");
        }

        if (key.Length != KeyBytes)
            throw new InvalidOperationException(
                $"'{path}' must decode to {KeyBytes} bytes for AES-256; got {key.Length}.");

        return key;
    }

    public string Encrypt(string plaintext)
    {
        // Null and empty round-trip untouched. Encrypting "" would turn an absent
        // note into a value, and every nullable column here treats them alike.
        if (string.IsNullOrEmpty(plaintext)) return plaintext;

        // Already encrypted: re-encrypting would nest the envelope and make the
        // data migration destructive if it ever ran twice.
        if (plaintext.StartsWith(Version, StringComparison.Ordinal)) return plaintext;

        var bytes = Encoding.UTF8.GetBytes(plaintext);

        // Fresh per encryption. Reusing a nonce under one key is the single fatal
        // mistake in GCM — it leaks the XOR of the two plaintexts.
        var nonce = RandomNumberGenerator.GetBytes(NonceBytes);
        var ciphertext = new byte[bytes.Length];
        var tag = new byte[TagBytes];

        using (var aes = new AesGcm(_key, TagBytes))
            aes.Encrypt(nonce, bytes, ciphertext, tag);

        // nonce || ciphertext || tag, so decryption needs nothing but the key.
        var envelope = new byte[NonceBytes + ciphertext.Length + TagBytes];
        nonce.CopyTo(envelope, 0);
        ciphertext.CopyTo(envelope, NonceBytes);
        tag.CopyTo(envelope, NonceBytes + ciphertext.Length);

        return Version + Convert.ToBase64String(envelope);
    }

    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext)) return ciphertext;

        // No prefix means the row predates encryption. Returning it unchanged is
        // what lets the data migration run against a half-converted table, and be
        // re-run safely after a failure part-way through.
        if (!ciphertext.StartsWith(Version, StringComparison.Ordinal)) return ciphertext;

        var envelope = Convert.FromBase64String(ciphertext[Version.Length..]);

        var nonce = envelope.AsSpan(0, NonceBytes);
        var tag = envelope.AsSpan(envelope.Length - TagBytes, TagBytes);
        var payload = envelope.AsSpan(NonceBytes, envelope.Length - NonceBytes - TagBytes);

        var plaintext = new byte[payload.Length];

        // Throws CryptographicException if the tag does not match — tampering, or
        // the wrong key. Deliberately not caught: silently returning the ciphertext
        // would put base64 into a user's report and call it their name.
        using (var aes = new AesGcm(_key, TagBytes))
            aes.Decrypt(nonce, payload, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    public string BlindIndex(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        // Must match User.NormalizeEmail exactly. If the two ever diverge, lookups
        // miss silently — a registered user simply appears not to exist.
        var normalized = value.Trim().ToLowerInvariant();

        var hash = HMACSHA256.HashData(_blindIndexKey, Encoding.UTF8.GetBytes(normalized));

        return Convert.ToHexStringLower(hash);
    }
}
