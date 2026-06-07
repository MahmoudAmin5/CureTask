using Cure.Domain.Common;
using Cure.Domain.Entities;
using Cure.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace Cure.Infrastructure.Interceptors;

public sealed class AuditLoggingInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not CureDbContext context)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var entries = context.ChangeTracker
    .Entries()
    .Where(e => e.Entity is not AuditLog &&
                e.Entity is Entity && // <-- ONLY track entities that inherit from your base Entity class
               (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
    .ToList();

        foreach (var entry in entries)
        {
            var entityName = entry.Entity.GetType().Name;
            var entityId = entry.Property("Id").CurrentValue?.ToString() ?? string.Empty;

            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Modified",
                EntityState.Deleted => "Deleted",
                _ => string.Empty
            };

            string? oldValues = null;
            string? newValues = null;

            switch (entry.State)
            {
                case EntityState.Added:
                    newValues = JsonSerializer.Serialize(
                        entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
                    break;

                case EntityState.Modified:
                    oldValues = JsonSerializer.Serialize(
                        entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
                    newValues = JsonSerializer.Serialize(
                        entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
                    break;

                case EntityState.Deleted:
                    oldValues = JsonSerializer.Serialize(
                        entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
                    break;
            }

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                OldValues = oldValues,
                NewValues = newValues,
                Timestamp = DateTime.UtcNow
            };

            context.AuditLogs.Add(auditLog);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
