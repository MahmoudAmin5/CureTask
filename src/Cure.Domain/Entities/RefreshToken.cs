using Cure.Domain.Common;

namespace Cure.Domain.Entities;

public sealed class RefreshToken : Entity
{
    public string UserId { get; set; } = null!;

    public string Token { get; set; } = null!;

    public DateTime ExpiresAtUtc { get; set; }

    public new DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAtUtc { get; set; }

    public string? ReplacedByToken { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

    public bool IsRevoked => RevokedAtUtc != null;

    public bool IsActive => !IsRevoked && !IsExpired;
}
