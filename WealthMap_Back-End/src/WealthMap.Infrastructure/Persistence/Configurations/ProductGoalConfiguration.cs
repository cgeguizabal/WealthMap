using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class ProductGoalConfiguration : IEntityTypeConfiguration<ProductGoal>
{
    public void Configure(EntityTypeBuilder<ProductGoal> builder)
    {
        builder.ToTable("product_goals");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(120);

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

        builder.HasIndex(g => g.UserId);
    }
}
