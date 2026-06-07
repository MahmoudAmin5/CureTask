using Cure.Domain.Common;

namespace Cure.Domain.Entities;

public sealed class AuditLog : Entity
{
    public string EntityName { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? UserId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
