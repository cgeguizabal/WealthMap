using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCard>
{
    public void Configure(EntityTypeBuilder<CreditCard> builder)
    {
        builder.ToTable("credit_cards");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CardName)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(c => c.BankName)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(c => c.AnnualInterestRate)
            .IsRequired()
            .HasColumnType("numeric(6,3)");

        builder.Property(c => c.PaymentDueDay)
            .IsRequired();

        builder.Property(c => c.StatementCutoffDay)
            .IsRequired();

        builder.Property(c => c.Notes)
            .HasMaxLength(1000);

        builder.ComplexProperty(c => c.CreditLimit, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("credit_limit")
                 .HasColumnType("numeric(18,2)")
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("currency")
                 .HasMaxLength(3)
                 .IsFixedLength()
                 .IsRequired();
        });

        builder.ComplexProperty(c => c.UsedCredit, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("used_credit")
                 .HasColumnType("numeric(18,2)")
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("used_credit_currency")
                 .HasMaxLength(3)
                 .IsFixedLength()
                 .IsRequired();
        });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.UserId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_credit_cards_used_within_limit", "used_credit <= credit_limit");
            t.HasCheckConstraint("ck_credit_cards_due_day", "payment_due_day BETWEEN 1 AND 31");
            t.HasCheckConstraint("ck_credit_cards_cutoff_day", "statement_cutoff_day BETWEEN 1 AND 31");
        });
    }
}