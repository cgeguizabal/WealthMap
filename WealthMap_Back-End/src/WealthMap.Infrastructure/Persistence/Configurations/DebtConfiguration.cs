using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class DebtConfiguration : IEntityTypeConfiguration<Debt>
{
    private readonly IEncryptionService _encryption;

    public DebtConfiguration(IEncryptionService encryption) => _encryption = encryption;

    public void Configure(EntityTypeBuilder<Debt> builder)
    {
        builder.ToTable("debts", t =>
        {
            t.HasCheckConstraint("ck_debts_remaining_within_original", "remaining_amount <= original_amount");
            t.HasCheckConstraint("ck_debts_due_day", "monthly_due_day BETWEEN 1 AND 31");
        });

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .IsEncrypted(_encryption);

        builder.Property(d => d.MonthlyDueDay)
            .IsRequired();

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.ComplexProperty(d => d.OriginalAmount, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("original_amount")
                 .HasColumnType("numeric(18,2)")
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("currency")
                 .HasMaxLength(3)
                 .IsFixedLength()
                 .IsRequired();
        });

        builder.ComplexProperty(d => d.RemainingAmount, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("remaining_amount")
                 .HasColumnType("numeric(18,2)")
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("remaining_amount_currency")
                 .HasMaxLength(3)
                 .IsFixedLength()
                 .IsRequired();
        });

        builder.ComplexProperty(d => d.MonthlyPayment, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("monthly_payment")
                 .HasColumnType("numeric(18,2)")
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("monthly_payment_currency")
                 .HasMaxLength(3)
                 .IsFixedLength()
                 .IsRequired();
        });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.UserId);
    }
}
