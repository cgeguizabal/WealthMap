using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class InstallmentPurchaseConfiguration : IEntityTypeConfiguration<InstallmentPurchase>
{
    public void Configure(EntityTypeBuilder<InstallmentPurchase> builder)
    {
        builder.ToTable("installment_purchases", t =>
            t.HasCheckConstraint("ck_installment_purchases_months", "months_count BETWEEN 1 AND 120"));

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.MonthsCount)
            .IsRequired();

        builder.Property(p => p.PurchasedAt)
            .IsRequired();

        builder.ComplexProperty(p => p.TotalPrice, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("total_price")
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

        builder.HasOne<CreditCard>()
            .WithMany()
            .HasForeignKey(p => p.CreditCardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Store>()
            .WithMany()
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Payments)
            .WithOne()
            .HasForeignKey(i => i.InstallmentPurchaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Payments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(p => p.UserId);
    }
}
