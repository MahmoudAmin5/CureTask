using Cure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cure.Infrastructure.Data.Configurations;

public sealed class PatientFileConfiguration : IEntityTypeConfiguration<PatientFile>
{
    public void Configure(EntityTypeBuilder<PatientFile> builder)
    {
        builder.HasKey(pf => pf.Id);

        builder.HasOne(a => a.Patient)
            .WithMany(p => p.PatientFiles)
            .HasForeignKey(pf => pf.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(pf => pf.FileName)
            .IsRequired();

        builder.Property(pf => pf.ContentType)
            .IsRequired();

        builder.Property(pf => pf.FilePath)
            .IsRequired();
    }
}
