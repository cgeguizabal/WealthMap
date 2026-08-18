using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WealthMap.Application.Common.Interfaces;

namespace WealthMap.Infrastructure.Persistence;

/// <summary>
/// Applies the encryption converter to a column, in one line per property.
/// </summary>
/// <remarks>
/// Per-property rather than a convention over "every string": most strings here
/// must stay queryable. Bank names are matched against bank_defaults, currencies
/// are compared, categories are filtered. Encrypting by default would break those
/// silently, so each encrypted column is named deliberately in its own
/// configuration.
///
/// The domain knows nothing about any of this. Entities hold plaintext; the
/// conversion happens between the model and the database, which is the only layer
/// that should care.
/// </remarks>
public static class EncryptedColumn
{
    /// <summary>Encrypts a text column, nullable or not.</summary>
    /// <remarks>
    /// Generic in the property type, and converting through a non-generic
    /// <see cref="ValueConverter"/>, because <c>PropertyBuilder&lt;string&gt;</c>
    /// and <c>PropertyBuilder&lt;string?&gt;</c> are the same type once the
    /// nullable annotation is erased — they can be neither overloaded nor passed
    /// to one another without a warning. The CLR type both carry is plain
    /// <c>string</c>, which is what the converter is matched against.
    ///
    /// Null needs no special case: EF does not run a value converter on null, so a
    /// null column stays null in the database. That matters more than it looks —
    /// the debit-card and tracking-mode CHECK constraints both test IS NULL, and an
    /// encrypted empty string would quietly satisfy them.
    /// </remarks>
    public static PropertyBuilder<TProperty> IsEncrypted<TProperty>(
        this PropertyBuilder<TProperty> builder, IEncryptionService encryption) =>
        builder
            .HasConversion(new ValueConverter<string, string>(
                value => encryption.Encrypt(value),
                value => encryption.Decrypt(value)))
            // Ciphertext runs about a third longer than the plaintext plus a
            // 28-byte envelope, so every length cap the plaintext carried is now
            // wrong. text has no cap and costs nothing extra in Postgres.
            .HasColumnType("text");
}
