using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId).IsRequired();

        // SHA-256 as hex is always 64 characters.
        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(64)
            .IsFixedLength();

        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Property(t => t.RevokedAt);

        builder.Property(t => t.ReplacedByTokenHash)
            .HasMaxLength(64)
            .IsFixedLength();

        // Every refresh is a lookup by hash, so this index is on the hot path. Unique
        // because two rows sharing a hash would make "which token is this?" ambiguous
        // at exactly the moment the answer decides whether to end every session.
        builder.HasIndex(t => t.TokenHash).IsUnique();

        // Supports the revoke-all sweep after a replay is detected.
        builder.HasIndex(t => t.UserId);

        // Deleting the user takes their sessions with them.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
