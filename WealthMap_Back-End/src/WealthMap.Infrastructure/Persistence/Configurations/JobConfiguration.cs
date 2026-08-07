using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("jobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Title)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(j => j.Employer)
            .IsRequired()
            .HasMaxLength(120);

        builder.ComplexProperty(j => j.GrossMonthlySalary, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("gross_monthly_salary")
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
            .HasForeignKey(j => j.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(j => j.DepositAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(j => j.PaymentDays)
            .WithOne()
            .HasForeignKey(d => d.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(j => j.Deductions)
            .WithOne()
            .HasForeignKey(d => d.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(j => j.PaymentDays)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(j => j.Deductions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(j => j.UserId);
    }
}