using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments", t =>
            t.HasCheckConstraint(
                "ck_payments_source_account",
                // 1 = Account, 2 = External: the account column must match the source.
                "(source_type = 1 AND source_account_id IS NOT NULL) OR "
                + "(source_type = 2 AND source_account_id IS NULL)"));

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TargetType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.SourceType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.TargetId)
            .IsRequired();

        builder.Property(p => p.OccurredAt)
            .IsRequired();

        builder.Property(p => p.Notes)
            .HasMaxLength(300);

        builder.ComplexProperty(p => p.Amount, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("amount")
                 .HasColumnType("numeric(18,2)")
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("currency")
                 .HasMaxLength(3)
                 .IsFixedLength()
                 .IsRequired();
        });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // TargetId is polymorphic across three tables, so it carries no FK. The source
        // account does: an account referenced by a payment must not disappear.
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(p => p.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.UserId, p.OccurredAt });
        builder.HasIndex(p => new { p.TargetType, p.TargetId });
    }
}