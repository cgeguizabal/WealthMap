using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class SalaryDepositConfiguration : IEntityTypeConfiguration<SalaryDeposit>
{
    public void Configure(EntityTypeBuilder<SalaryDeposit> builder)
    {
        builder.ToTable("salary_deposits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.AccountId).IsRequired();
        builder.Property(x => x.ScheduledDate).IsRequired();
        builder.Property(x => x.PostedAt).IsRequired();
        builder.Property(x => x.AccountMovementId).IsRequired();

        builder.ComplexProperty(x => x.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        });

        // The guarantee that a payday is paid once. The service checks first, but two
        // instances can pass that check simultaneously; only the database can settle it.
        builder.HasIndex(x => new { x.JobId, x.ScheduledDate }).IsUnique();

        builder.HasIndex(x => x.UserId);

        builder.HasOne<Job>()
            .WithMany()
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
