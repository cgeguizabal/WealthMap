using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class AccountMovementConfiguration : IEntityTypeConfiguration<AccountMovement>
{
    public void Configure(EntityTypeBuilder<AccountMovement> builder)
    {
        builder.ToTable("account_movements");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(m => m.Location)
            .HasMaxLength(200);

        builder.Property(m => m.OccurredAt)
            .IsRequired();

        builder.ComplexProperty(m => m.Amount, money =>
        {
            money.Property(x => x.Amount)
                 .HasColumnName("amount")
                 .HasColumnType("numeric(18,2)")
                 .IsRequired();

            money.Property(x => x.Currency)
                 .HasColumnName("currency")
                 .HasMaxLength(3)
                 .IsFixedLength()
                 .IsRequired();
        });

        builder.ComplexProperty(m => m.BalanceAfter, money =>
        {
            money.Property(x => x.Amount)
                 .HasColumnName("balance_after")
                 .HasColumnType("numeric(18,2)")
                 .IsRequired();

            money.Property(x => x.Currency)
                 .HasColumnName("balance_after_currency")
                 .HasMaxLength(3)
                 .IsFixedLength()
                 .IsRequired();
        });

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(m => m.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.UserId, m.OccurredAt })
            .HasDatabaseName("ix_account_movements_user_occurred");

        builder.HasIndex(m => new { m.AccountId, m.OccurredAt })
            .HasDatabaseName("ix_account_movements_account_occurred");
    }
}