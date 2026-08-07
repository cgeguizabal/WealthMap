using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class JobPaymentDayConfiguration : IEntityTypeConfiguration<JobPaymentDay>
{
    public void Configure(EntityTypeBuilder<JobPaymentDay> builder)
    {
        builder.ToTable("job_payment_days", t =>
            t.HasCheckConstraint("ck_job_payment_days_day_of_month", "day_of_month BETWEEN 1 AND 31"));

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DayOfMonth)
            .IsRequired();

        builder.HasIndex(d => d.JobId);
    }
}