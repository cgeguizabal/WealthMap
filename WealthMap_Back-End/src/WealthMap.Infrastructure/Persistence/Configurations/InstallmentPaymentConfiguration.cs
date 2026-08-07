using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class InstallmentPaymentConfiguration : IEntityTypeConfiguration<InstallmentPayment>
{
    public void Configure(EntityTypeBuilder<InstallmentPayment> builder)
    {
        builder.ToTable("installment_payments", t =>
            t.HasCheckConstraint("ck_installment_payments_number", "number >= 1"));

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Number)
            .IsRequired();

        builder.Property(p => p.DueDate)
            .IsRequired();

        builder.Property(p => p.IsPaid)
            .IsRequired();

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

        builder.HasIndex(p => new { p.InstallmentPurchaseId, p.Number })
            .IsUnique();
    }
}
