using Cure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cure.Infrastructure.Data.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.HasIndex(p => p.UserId)
            .IsUnique();

        builder.Property(p => p.DateOfBirth)
            .IsRequired();

        builder.Property(p => p.Gender)
            .IsRequired();

        builder.Property(p => p.Address)
            .IsRequired();

        builder.Property(p => p.BloodType);

        builder.Property(p => p.EmergencyContact);

        builder.HasMany(p => p.Appointments)
            .WithOne()
            .HasForeignKey(a => a.PatientId);

        builder.HasMany(p => p.MedicalHistories)
            .WithOne()
            .HasForeignKey(mh => mh.PatientId);

        builder.HasMany(p => p.ClinicalNotes)
            .WithOne()
            .HasForeignKey(cn => cn.PatientId);

        builder.HasMany(p => p.PatientFiles)
            .WithOne()
            .HasForeignKey(pf => pf.PatientId);
    }
}
