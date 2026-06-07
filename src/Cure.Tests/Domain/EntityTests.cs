using Cure.Domain.Common;
using Cure.Domain.Entities;
using FluentAssertions;

namespace Cure.Tests.Domain;

public class EntityTests
{
    [Fact]
    public void Appointment_ShouldInheritFromEntity()
    {
        // Arrange & Act
        var appointment = new Appointment();

        // Assert
        appointment.Should().BeAssignableTo<Entity>();
        appointment.Id.Should().Be(default(Guid));
        appointment.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Patient_ShouldInheritFromEntity()
    {
        // Arrange & Act
        var patient = new Patient();

        // Assert
        patient.Should().BeAssignableTo<Entity>();
        patient.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RefreshToken_IsActive_ShouldReturnTrue_WhenNotExpiredAndNotRevoked()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "test-token",
            UserId = "user-1",
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1),
            RevokedAtUtc = null
        };

        // Act & Assert
        token.IsActive.Should().BeTrue();
        token.IsExpired.Should().BeFalse();
        token.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_IsExpired_ShouldReturnTrue_WhenPastExpirationDate()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "expired-token",
            UserId = "user-1",
            ExpiresAtUtc = DateTime.UtcNow.AddHours(-1),
            RevokedAtUtc = null
        };

        // Act & Assert
        token.IsExpired.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_IsRevoked_ShouldReturnTrue_WhenRevokedAtUtcIsSet()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "revoked-token",
            UserId = "user-1",
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1),
            RevokedAtUtc = DateTime.UtcNow.AddMinutes(-10)
        };

        // Act & Assert
        token.IsRevoked.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }
}
