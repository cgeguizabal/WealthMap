using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;

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

        builder.Property(c => c.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);

        // char(4): the value is always four digits or absent, never a range.
        builder.Property(c => c.LastFour)
            .HasMaxLength(4)
            .IsFixedLength();

        // The sentinel is the CLR default, 0, which is not a member of the enum.
        // Without it EF cannot tell "the caller left this alone" from "the caller
        // chose Manual", and warns that the database default would win either way.
        // Declaring 0 as the sentinel makes the constructor's Manual an explicit
        // write, while the column default still backfills existing rows.
        builder.Property(c => c.TrackingMode)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(TrackingMode.Manual)
            .HasSentinel(default(TrackingMode));

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

            // Defence in depth behind CreditCard.SetLastFour / SetTrackingMode. The
            // entity gives the readable error; the constraint means a row claiming to
            // be synced without identifying digits cannot exist even if written by a
            // script or a future migration that forgets the rule.
            t.HasCheckConstraint(
                "ck_credit_cards_sync_requires_last_four",
                "(tracking_mode = 1) OR (last_four IS NOT NULL)");
        });
    }
}