using Cure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cure.Infrastructure.Data.Configurations;

public sealed class ClinicalNoteConfiguration : IEntityTypeConfiguration<ClinicalNote>
{
    public void Configure(EntityTypeBuilder<ClinicalNote> builder)
    {
        builder.HasKey(cn => cn.Id);

        builder.HasOne(a => a.Patient)
            .WithMany(p => p.ClinicalNotes)
            .HasForeignKey(cn => cn.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(cn => cn.AuthorId)
            .IsRequired();

        builder.HasIndex(cn => cn.AuthorId);

        builder.Property(cn => cn.Content)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(cn => cn.NoteType)
            .IsRequired()
            .HasMaxLength(100);
    }
}
