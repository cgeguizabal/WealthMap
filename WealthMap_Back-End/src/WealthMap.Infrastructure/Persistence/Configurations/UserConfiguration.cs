using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>Shadow property name, referenced by the repository through EF.Property.</summary>
    public const string EmailLookup = "EmailLookup";

    private readonly IEncryptionService _encryption;

    public UserConfiguration(IEncryptionService encryption) => _encryption = encryption;

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .IsEncrypted(_encryption);

        // A shadow property, not a domain member. The blind index exists only
        // because the column above is encrypted, and the domain is not allowed to
        // learn that. `User` has no EmailLookup; only the model does.
        builder.Property<string>(UserConfiguration.EmailLookup)
            .HasColumnName("email_lookup")
            .IsRequired()
            .HasMaxLength(64)
            .IsFixedLength();

        // The uniqueness that `email` used to carry. Randomised ciphertext cannot
        // hold it: two encryptions of one address differ, so the database would
        // happily store the same person twice.
        builder.HasIndex(UserConfiguration.EmailLookup)
            .IsUnique()
            .HasDatabaseName("ix_users_email_lookup");

        // Already a one-way hash with its own salt; encrypting it would add
        // nothing and cost a decryption on every sign-in.
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(u => u.FullName)
            .IsRequired()
            .IsEncrypted(_encryption);

        builder.Property(u => u.Country)
            .IsRequired()
            .IsEncrypted(_encryption);

        // Not encrypted: every total in the app is aggregated per currency, and the
        // snapshot filters holdings by it. Three letters shared by whole countries
        // identify nobody.
        builder.Property(u => u.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength();

        builder.Property(u => u.TermsAcceptedAt);

        builder.Property(u => u.AcceptedPolicyVersion)
            .HasMaxLength(20);
    }
}
