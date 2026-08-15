using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;

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

        // char(4): the value is always four digits or absent, never a range.
        builder.Property(a => a.LastFour)
            .HasMaxLength(4)
            .IsFixedLength();

        // The sentinel is the CLR default, 0, which is not a member of the enum.
        // Without it EF cannot tell "the caller left this alone" from "the caller
        // chose Manual", and warns that the database default would win either way.
        // Declaring 0 as the sentinel makes the constructor's Manual an explicit
        // write, while the column default still backfills existing rows.
        builder.Property(a => a.TrackingMode)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(TrackingMode.Manual)
            .HasSentinel(default(TrackingMode));

        builder.Property(a => a.DebitCardLastFour)
            .HasMaxLength(4)
            .IsFixedLength();

        builder.Property(a => a.DebitCardType)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(DebitCardType.None)
            .HasSentinel(default(DebitCardType));

        builder.ToTable(t =>
        {
            // Defence in depth behind Account.SetLastFour / SetTrackingMode. The
            // entity gives the readable error; the constraint means a row claiming to
            // be synced without identifying digits cannot exist even if written by a
            // script or a future migration that forgets the rule.
            t.HasCheckConstraint(
                "ck_accounts_sync_requires_last_four",
                "(tracking_mode = 1) OR (last_four IS NOT NULL)");

            // The mirror of SetDebitCard: no card, no card number.
            t.HasCheckConstraint(
                "ck_accounts_no_card_no_digits",
                "(debit_card_type <> 1) OR (debit_card_last_four IS NULL)");
        });

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