using Cure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cure.Infrastructure.Data.Configurations;

public sealed class MedicalHistoryConfiguration : IEntityTypeConfiguration<MedicalHistory>
{
    public void Configure(EntityTypeBuilder<MedicalHistory> builder)
    {
        builder.HasKey(mh => mh.Id);

        builder.HasOne(a => a.Patient)
            .WithMany(p => p.MedicalHistories)
            .HasForeignKey(mh => mh.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(mh => mh.Condition)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(mh => mh.Treatment)
            .HasMaxLength(1000);
    }
}
