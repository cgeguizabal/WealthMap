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
    /// The key generation that produced a value, stamped into every one.
    /// </summary>
    /// <remarks>
    /// Mandatory, and worth the four bytes. It is what makes rotation possible
    /// without downtime: values written by the outgoing key still say so, and are
    /// still readable, while everything new carries the current generation. It
    /// also lets <see cref="Decrypt"/> recognise plaintext that predates
    /// encryption entirely.
    /// </remarks>
    private readonly string _versionPrefix;
    private readonly int _version;

    // GCM's standard nonce size. Twelve bytes is what the construction is defined
    // for; other lengths force an extra hashing step and weaken the security proof.
    private const int NonceBytes = 12;
    private const int TagBytes = 16;
    private const int KeyBytes = 32;

    private readonly byte[] _key;
    private readonly byte[] _blindIndexKey;

    /// <summary>The outgoing key during a rotation. Decrypts only; never writes.</summary>
    private readonly byte[]? _previousKey;

    private readonly byte[]? _previousBlindIndexKey;

    public AesGcmEncryptionService(IOptions<EncryptionSettings> options)
    {
        var settings = options.Value;

        _key = ReadKey(settings.Key, $"{EncryptionSettings.SectionName}:Key");
        _blindIndexKey = ReadKey(
            settings.BlindIndexKey, $"{EncryptionSettings.SectionName}:BlindIndexKey");

        _version = settings.KeyVersion > 0
            ? settings.KeyVersion
            : throw new InvalidOperationException(
                $"'{EncryptionSettings.SectionName}:KeyVersion' must be 1 or greater.");

        _versionPrefix = $"v{_version}:";

        // Optional, and only present while a rotation is in flight.
        _previousKey = OptionalKey(
            settings.PreviousKey, $"{EncryptionSettings.SectionName}:PreviousKey");

        _previousBlindIndexKey = OptionalKey(
            settings.PreviousBlindIndexKey,
            $"{EncryptionSettings.SectionName}:PreviousBlindIndexKey");

        if (_previousKey is not null && _version == 1)
            throw new InvalidOperationException(
                $"'{EncryptionSettings.SectionName}:PreviousKey' is set but KeyVersion is 1, " +
                "so there is no earlier generation for it to read. Raise KeyVersion to 2.");
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

    /// <summary>A key that may legitimately be absent.</summary>
    private static byte[]? OptionalKey(string configured, string path) =>
        string.IsNullOrWhiteSpace(configured) ? null : ReadKey(configured, path);

    public string CurrentVersionPrefix => _versionPrefix;

    /// <summary>
    /// Which key wrote a value, from its stamp.
    /// </summary>
    /// <remarks>
    /// Throws rather than guessing. A value stamped with a generation this
    /// process has no key for is not corrupt — it is a deployment that was given
    /// the wrong configuration, and reading it as garbage would be worse than
    /// stopping.
    /// </remarks>
    private byte[] KeyForVersion(int version)
    {
        if (version == _version) return _key;

        if (version == _version - 1 && _previousKey is not null) return _previousKey;

        throw new InvalidOperationException(
            $"A stored value was written with key generation v{version}, but this app is " +
            $"configured with v{_version}" +
            (_previousKey is null ? " and no previous key." : $" and a previous key for v{_version - 1}.") +
            " Set Encryption:PreviousKey to the key that wrote it.");
    }

    /// <summary>Reads the <c>vN:</c> stamp. False means the value is plaintext.</summary>
    private static bool TryReadVersion(string value, out int version, out string body)
    {
        version = 0;
        body = value;

        if (value.Length < 3 || value[0] != 'v') return false;

        var colon = value.IndexOf(':');
        if (colon < 2) return false;

        if (!int.TryParse(value.AsSpan(1, colon - 1), out version)) return false;

        body = value[(colon + 1)..];
        return true;
    }

    public string Encrypt(string plaintext)
    {
        // Null and empty round-trip untouched. Encrypting "" would turn an absent
        // note into a value, and every nullable column here treats them alike.
        if (string.IsNullOrEmpty(plaintext)) return plaintext;

        // Already encrypted, by any generation: re-encrypting would nest the
        // envelope and make the data pass destructive if it ever ran twice.
        //
        // Rotation does not go through here — it reads a value (decrypting with
        // the old key) and writes it back (encrypting with the new one), so what
        // arrives is plaintext.
        if (TryReadVersion(plaintext, out _, out _)) return plaintext;

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

        return _versionPrefix + Convert.ToBase64String(envelope);
    }

    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext)) return ciphertext;

        // No stamp means the row predates encryption. Returning it unchanged is
        // what lets the data pass run against a half-converted table, and be
        // re-run safely after a failure part-way through.
        if (!TryReadVersion(ciphertext, out var version, out var body)) return ciphertext;

        // Which key wrote it. During a rotation this is the previous key for rows
        // the pass has not reached yet, and the current one for everything else.
        var key = KeyForVersion(version);

        var envelope = Convert.FromBase64String(body);

        var nonce = envelope.AsSpan(0, NonceBytes);
        var tag = envelope.AsSpan(envelope.Length - TagBytes, TagBytes);
        var payload = envelope.AsSpan(NonceBytes, envelope.Length - NonceBytes - TagBytes);

        var plaintext = new byte[payload.Length];

        // Throws CryptographicException if the tag does not match — tampering, or
        // the wrong key. Deliberately not caught: silently returning the ciphertext
        // would put base64 into a user's report and call it their name.
        using (var aes = new AesGcm(key, TagBytes))
            aes.Decrypt(nonce, payload, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    public string BlindIndex(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        // Must match User.NormalizeEmail exactly. If the two ever diverge, lookups
        // miss silently — a registered user simply appears not to exist.
        var normalized = value.Trim().ToLowerInvariant();

        return Hash(_blindIndexKey, normalized);
    }

    public IReadOnlyList<string> BlindIndexCandidates(string value)
    {
        if (string.IsNullOrEmpty(value)) return [string.Empty];

        var normalized = value.Trim().ToLowerInvariant();

        // Current key first: at rest that is the only one, and during a rotation
        // it is the one most rows will already carry.
        return _previousBlindIndexKey is null
            ? [Hash(_blindIndexKey, normalized)]
            : [Hash(_blindIndexKey, normalized), Hash(_previousBlindIndexKey, normalized)];
    }

    private static string Hash(byte[] key, string normalized) =>
        Convert.ToHexStringLower(HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(normalized)));
}
