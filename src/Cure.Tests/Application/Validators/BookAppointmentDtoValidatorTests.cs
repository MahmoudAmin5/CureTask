using Cure.Application.DTOs.Appointment;
using Cure.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Cure.Tests.Application.Validators;

public class BookAppointmentDtoValidatorTests
{
    private readonly BookAppointmentDtoValidator _validator = new();

    private static BookAppointmentDto CreateValidDto() => new(
        PatientId: Guid.NewGuid(),
        NurseId: Guid.NewGuid(),
        ScheduledAtUtc: DateTime.UtcNow.AddDays(1),
        DurationMinutes: 30,
        Location: "Room 101",
        Notes: "Routine checkup"
    );

    [Fact]
    public void ValidDto_ShouldPassValidation()
    {
        // Arrange
        var dto = CreateValidDto();

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyPatientId_ShouldFailValidation()
    {
        // Arrange
        var dto = CreateValidDto() with { PatientId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PatientId);
    }

    [Fact]
    public void EmptyNurseId_ShouldFailValidation()
    {
        // Arrange
        var dto = CreateValidDto() with { NurseId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NurseId);
    }

    [Fact]
    public void PastDate_ShouldFailValidation()
    {
        // Arrange
        var dto = CreateValidDto() with { ScheduledAtUtc = DateTime.UtcNow.AddDays(-1) };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ScheduledAtUtc);
    }

    [Fact]
    public void DurationTooShort_ShouldFailValidation()
    {
        // Arrange
        var dto = CreateValidDto() with { DurationMinutes = 10 };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void DurationTooLong_ShouldFailValidation()
    {
        // Arrange
        var dto = CreateValidDto() with { DurationMinutes = 500 };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void EmptyLocation_ShouldFailValidation()
    {
        // Arrange
        var dto = CreateValidDto() with { Location = "" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Location);
    }
}
