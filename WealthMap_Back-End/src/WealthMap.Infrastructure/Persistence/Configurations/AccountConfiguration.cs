using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(a => a.BankName)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.Property(a => a.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);

        builder.ComplexProperty(a => a.Balance, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("balance")
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
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.UserId);
    }
}