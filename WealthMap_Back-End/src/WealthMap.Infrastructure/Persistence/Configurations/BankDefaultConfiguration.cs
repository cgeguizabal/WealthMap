using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class BankDefaultConfiguration : IEntityTypeConfiguration<BankDefault>
{
    public void Configure(EntityTypeBuilder<BankDefault> builder)
    {
        builder.ToTable("bank_defaults");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.UserId).IsRequired();

        builder.Property(b => b.BankName)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(b => b.Direction)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(b => b.DefaultAccountId).IsRequired();

        // One answer per bank per direction. Without this the upsert would have no
        // key to settle on and a user could end up with two contradictory fallbacks
        // for the same message.
        builder.HasIndex(b => new { b.UserId, b.BankName, b.Direction }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, not Cascade: a default silently disappearing when its account is
        // removed would leave the bank with no fallback and no sign that it ever had
        // one. The account has to be dealt with deliberately.
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(b => b.DefaultAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
