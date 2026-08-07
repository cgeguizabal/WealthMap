using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class DeductionConfiguration : IEntityTypeConfiguration<Deduction>
{
    public void Configure(EntityTypeBuilder<Deduction> builder)
    {
        builder.ToTable("deductions", t =>
            t.HasCheckConstraint("ck_deductions_value_positive", "value > 0"));

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(d => d.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.Value)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.HasIndex(d => d.JobId);
    }
}