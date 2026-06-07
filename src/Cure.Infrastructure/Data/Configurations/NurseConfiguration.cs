using Cure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cure.Infrastructure.Data.Configurations;

public sealed class NurseConfiguration : IEntityTypeConfiguration<Nurse>
{
    public void Configure(EntityTypeBuilder<Nurse> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.UserId)
            .IsRequired();

        builder.HasIndex(n => n.UserId)
            .IsUnique();

        builder.Property(n => n.Department)
            .IsRequired();

        builder.Property(n => n.LicenseNumber)
            .IsRequired();

        builder.HasMany(n => n.Appointments)
            .WithOne()
            .HasForeignKey(a => a.NurseId);
    }
}
