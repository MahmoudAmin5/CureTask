using Cure.Domain.Entities;
using Cure.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cure.Infrastructure.Data.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Nurse)
            .WithMany(n => n.Appointments)
            .HasForeignKey(a => a.NurseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.Status)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<AppointmentStatus>(v))
            .IsRequired();

        builder.Property(a => a.RowVersion)
            .IsRowVersion();

        builder.Property(a => a.Location)
            .HasMaxLength(200);

        builder.Property(a => a.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(a => a.PatientId);
        builder.HasIndex(a => a.NurseId);
        builder.HasIndex(a => a.ScheduledAtUtc);
    }
}
