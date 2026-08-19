using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class CardIncidentConfiguration : IEntityTypeConfiguration<CardIncident>
{
    private readonly IEncryptionService _encryption;

    public CardIncidentConfiguration(IEncryptionService encryption) => _encryption = encryption;

    public void Configure(EntityTypeBuilder<CardIncident> builder)
    {
        builder.ToTable("card_incidents");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Kind).IsRequired();
        builder.Property(i => i.CardId).IsRequired();
        builder.Property(i => i.Reason).IsRequired();
        builder.Property(i => i.ReportedOn).IsRequired();
        builder.Property(i => i.ReplacedOn);
        builder.Property(i => i.RecoveredOn);

        // Both numbers are as identifying as the ones on the card and the account
        // they were copied from, and are encrypted to the same standard. A record of
        // which digits a user lost would otherwise be readable where the card's own
        // are not.
        builder.Property(i => i.LastFourAtReport)
            .IsEncrypted(_encryption);

        builder.Property(i => i.NewLastFour)
            .IsEncrypted(_encryption);

        builder.Property(i => i.Notes)
            .IsEncrypted(_encryption);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // No foreign key on CardId. It points at a credit card for one Kind and at
        // an account for the other, and a column cannot reference two tables. The
        // handlers load the card through its own user-scoped repository before
        // writing, so nothing reaches this table without the card having been found.
        builder.HasIndex(i => new { i.UserId, i.Kind, i.CardId });

        builder.ToTable(t =>
        {
            // Defence in depth behind RecordReplacement and RecordRecovery. The two
            // outcomes are alternatives, and a row carrying both would make Status
            // answer "replaced" while a recovery date sat beside it saying otherwise.
            t.HasCheckConstraint(
                "ck_card_incidents_one_outcome",
                "(replaced_on IS NULL) OR (recovered_on IS NULL)");

            // Neither outcome can predate the report that opened it.
            t.HasCheckConstraint(
                "ck_card_incidents_outcome_after_report",
                "(replaced_on IS NULL OR replaced_on >= reported_on) " +
                "AND (recovered_on IS NULL OR recovered_on >= reported_on)");
        });
    }
}
