using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class FreelanceJobConfiguration : IEntityTypeConfiguration<FreelanceJob>
{
    private readonly IEncryptionService _encryption;

    public FreelanceJobConfiguration(IEncryptionService encryption) => _encryption = encryption;

    public void Configure(EntityTypeBuilder<FreelanceJob> builder)
    {
        builder.ToTable("freelance_jobs");

        builder.HasKey(f => f.Id);

        // A project title names a client's work, and the client names the client.
        // Both are as identifying as an account name, so both are encrypted.
        builder.Property(f => f.Title)
            .IsRequired()
            .IsEncrypted(_encryption);

        builder.Property(f => f.Client)
            .IsEncrypted(_encryption);

        builder.Property(f => f.Notes)
            .IsEncrypted(_encryption);

        builder.Property(f => f.DueOn);
        builder.Property(f => f.DeliveredOn);
        builder.Property(f => f.PaidOn);
        builder.Property(f => f.CancelledOn);
        builder.Property(f => f.DepositAccountId);

        builder.ComplexProperty(f => f.AgreedAmount, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("agreed_amount")
                 .HasColumnType("numeric(18,2)")
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("currency")
                 .HasMaxLength(3)
                 .IsFixedLength()
                 .IsRequired();
        });

        builder.ComplexProperty(f => f.AmountPaid, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("amount_paid")
                 .HasColumnType("numeric(18,2)")
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("amount_paid_currency")
                 .HasMaxLength(3)
                 .IsFixedLength()
                 .IsRequired();
        });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, not SetNull: the deposit account is the evidence of where the
        // money went. An account that a payment landed in cannot quietly vanish
        // and leave the payment pointing nowhere.
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(f => f.DepositAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.UserId);

        builder.ToTable(t =>
        {
            // Defence in depth behind MarkPaid. The three facts of a payment
            // arrive together or not at all — a row with a paid date but no
            // account, or an amount with no date, is not a state the domain can
            // produce and should not be one the database can hold.
            t.HasCheckConstraint(
                "ck_freelance_jobs_paid_together",
                "(paid_on IS NULL AND deposit_account_id IS NULL AND amount_paid = 0) " +
                "OR (paid_on IS NOT NULL AND deposit_account_id IS NOT NULL AND amount_paid > 0)");

            // Cancelled work is never paid work; MarkPaid and Cancel each refuse
            // the other, and this is the same rule where a script cannot miss it.
            t.HasCheckConstraint(
                "ck_freelance_jobs_not_paid_and_cancelled",
                "(paid_on IS NULL) OR (cancelled_on IS NULL)");
        });
    }
}
