using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class SavingsGoalConfiguration : IEntityTypeConfiguration<SavingsGoal>
{
    private readonly IEncryptionService _encryption;

    public SavingsGoalConfiguration(IEncryptionService encryption) => _encryption = encryption;

    public void Configure(EntityTypeBuilder<SavingsGoal> builder)
    {
        builder.ToTable("savings_goals");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .IsRequired()
            .IsEncrypted(_encryption);

        builder.Property(g => g.Deadline)
            .IsRequired();

        builder.ComplexProperty(g => g.TargetAmount, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("target_amount")
                 .HasColumnType("numeric(18,2)")
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("currency")
                 .HasMaxLength(3)
                 .IsFixedLength()
                 .IsRequired();
        });

        builder.ComplexProperty(g => g.CurrentAmount, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("current_amount")
                 .HasColumnType("numeric(18,2)")
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("current_amount_currency")
                 .HasMaxLength(3)
                 .IsFixedLength()
                 .IsRequired();
        });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(g => g.LinkedAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(g => g.UserId);
    }
}
