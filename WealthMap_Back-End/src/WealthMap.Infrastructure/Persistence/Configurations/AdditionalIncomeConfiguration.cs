using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class AdditionalIncomeConfiguration : IEntityTypeConfiguration<AdditionalIncome>
{
    public void Configure(EntityTypeBuilder<AdditionalIncome> builder)
    {
        builder.ToTable("additional_incomes");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(i => i.Frequency)
            .IsRequired()
            .HasConversion<int>();

        builder.ComplexProperty(i => i.Amount, money =>
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
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(i => i.DepositAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.UserId);
    }
}