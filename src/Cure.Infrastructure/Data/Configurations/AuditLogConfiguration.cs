using Cure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cure.Infrastructure.Data.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(al => al.Id);

        builder.Property(al => al.EntityName)
            .IsRequired();

        builder.Property(al => al.EntityId)
            .IsRequired();

        builder.Property(al => al.Action)
            .IsRequired();

        builder.Property(al => al.OldValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(al => al.NewValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(al => al.UserId);

        builder.HasIndex(al => al.UserId);

        builder.HasIndex(al => al.Timestamp);
    }
}
