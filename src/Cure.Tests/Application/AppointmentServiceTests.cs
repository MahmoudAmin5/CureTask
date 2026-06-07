using System.Linq.Expressions;
using Cure.Application.Abstractions;
using Cure.Application.DTOs.Appointment;
using Cure.Application.Services;
using Cure.Domain.Common;
using Cure.Domain.Entities;
using Cure.Domain.Enums;
using Cure.Domain.Errors;
using Cure.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Cure.Tests.Application;

public class AppointmentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUoW;
    private readonly Mock<IValidator<BookAppointmentDto>> _mockValidator;
    private readonly Mock<ILogger<AppointmentService>> _mockLogger;
    private readonly Mock<IGenericRepository<Appointment>> _mockAppointmentRepo;
    private readonly Mock<IGenericRepository<Patient>> _mockPatientRepo;
    private readonly Mock<IGenericRepository<Nurse>> _mockNurseRepo;
    private readonly AppointmentService _sut;

    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _nurseId = Guid.NewGuid();
    private readonly DateTime _futureDate = DateTime.UtcNow.AddDays(1);

    public AppointmentServiceTests()
    {
        _mockUoW = new Mock<IUnitOfWork>();
        _mockValidator = new Mock<IValidator<BookAppointmentDto>>();
        _mockLogger = new Mock<ILogger<AppointmentService>>();
        _mockAppointmentRepo = new Mock<IGenericRepository<Appointment>>();
        _mockPatientRepo = new Mock<IGenericRepository<Patient>>();
        _mockNurseRepo = new Mock<IGenericRepository<Nurse>>();

        _mockUoW.Setup(u => u.Repository<Appointment>()).Returns(_mockAppointmentRepo.Object);
        _mockUoW.Setup(u => u.Repository<Patient>()).Returns(_mockPatientRepo.Object);
        _mockUoW.Setup(u => u.Repository<Nurse>()).Returns(_mockNurseRepo.Object);

        _sut = new AppointmentService(_mockUoW.Object, _mockValidator.Object, _mockLogger.Object);
    }

    private void SetupValidValidator()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<BookAppointmentDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private void SetupPatientExists()
    {
        _mockPatientRepo
            .Setup(r => r.GetByIdAsync(_patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Patient
            {
                Id = _patientId,
                UserId = "user-1",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male",
                Address = "123 Main St"
            });
    }

    private void SetupNurseExists()
    {
        _mockNurseRepo
            .Setup(r => r.GetByIdAsync(_nurseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Nurse
            {
                Id = _nurseId,
                UserId = "nurse-1",
                Department = "General",
                LicenseNumber = "LN-001",
                HireDate = new DateTime(2020, 1, 1)
            });
    }

    private void SetupNoOverlappingAppointments()
    {
        _mockAppointmentRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Appointment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>().AsReadOnly());
    }

    [Fact]
    public async Task BookAppointment_WithValidData_ShouldSucceed()
    {
        // Arrange
        SetupValidValidator();
        SetupNoOverlappingAppointments();
        SetupPatientExists();
        SetupNurseExists();

        _mockUoW.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _sut.BookAppointmentAsync(
            _patientId, _nurseId, _futureDate, 30, "Room 101", "Checkup");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be(_patientId);
        result.Value.NurseId.Should().Be(_nurseId);
        result.Value.ScheduledAtUtc.Should().Be(_futureDate);
        result.Value.DurationMinutes.Should().Be(30);
        result.Value.Location.Should().Be("Room 101");
        result.Value.Notes.Should().Be("Checkup");
        result.Value.Status.Should().Be(AppointmentStatus.Pending);

        _mockAppointmentRepo.Verify(r => r.Add(It.IsAny<Appointment>()), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUoW.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BookAppointment_WithInvalidData_ShouldReturnValidationFailure()
    {
        // Arrange
        var validationFailures = new List<ValidationFailure>
        {
            new("PatientId", "Patient ID is required."),
            new("Location", "Location is required.")
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<BookAppointmentDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var result = await _sut.BookAppointmentAsync(
            Guid.Empty, _nurseId, _futureDate, 30, "", null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task BookAppointment_WithPastDate_ShouldReturnPastDateError()
    {
        // Arrange
        SetupValidValidator();
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = await _sut.BookAppointmentAsync(
            _patientId, _nurseId, pastDate, 30, "Room 101", null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Appointment.PastDateNotAllowed);
    }

    [Fact]
    public async Task BookAppointment_WithOverlappingSlot_ShouldReturnDoubleBookingConflict()
    {
        // Arrange
        SetupValidValidator();

        var overlappingAppointments = new List<Appointment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                NurseId = _nurseId,
                PatientId = Guid.NewGuid(),
                ScheduledAtUtc = _futureDate,
                DurationMinutes = 60,
                Location = "Room 102",
                Status = AppointmentStatus.Confirmed
            }
        };

        _mockAppointmentRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Appointment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(overlappingAppointments.AsReadOnly());

        // Act
        var result = await _sut.BookAppointmentAsync(
            _patientId, _nurseId, _futureDate, 30, "Room 101", null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Appointment.DoubleBookingConflict);
        _mockUoW.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BookAppointment_WithNonExistentPatient_ShouldReturnPatientNotFound()
    {
        // Arrange
        SetupValidValidator();
        SetupNoOverlappingAppointments();
        SetupNurseExists();

        _mockPatientRepo
            .Setup(r => r.GetByIdAsync(_patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var result = await _sut.BookAppointmentAsync(
            _patientId, _nurseId, _futureDate, 30, "Room 101", null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Patient.NotFound);
        _mockUoW.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BookAppointment_WithNonExistentNurse_ShouldReturnNurseNotFound()
    {
        // Arrange
        SetupValidValidator();
        SetupNoOverlappingAppointments();
        SetupPatientExists();

        _mockNurseRepo
            .Setup(r => r.GetByIdAsync(_nurseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Nurse?)null);

        // Act
        var result = await _sut.BookAppointmentAsync(
            _patientId, _nurseId, _futureDate, 30, "Room 101", null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Nurse.NotFound);
        _mockUoW.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BookAppointment_OnConcurrencyException_ShouldRollbackAndReturnConflict()
    {
        // Arrange
        SetupValidValidator();
        SetupNoOverlappingAppointments();
        SetupPatientExists();
        SetupNurseExists();

        _mockUoW
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency conflict"));

        // Act
        var result = await _sut.BookAppointmentAsync(
            _patientId, _nurseId, _futureDate, 30, "Room 101", null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Appointment.DoubleBookingConflict);
        _mockUoW.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAppointment_WithValidId_ShouldSucceed()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = _patientId,
            NurseId = _nurseId,
            ScheduledAtUtc = _futureDate,
            DurationMinutes = 30,
            Location = "Room 101",
            Status = AppointmentStatus.Confirmed
        };

        _mockAppointmentRepo
            .Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _mockUoW.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _sut.CancelAppointmentAsync(appointmentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        _mockAppointmentRepo.Verify(r => r.Update(appointment), Times.Once);
        _mockUoW.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAppointment_WithAlreadyCancelled_ShouldReturnAlreadyCancelled()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = _patientId,
            NurseId = _nurseId,
            ScheduledAtUtc = _futureDate,
            DurationMinutes = 30,
            Location = "Room 101",
            Status = AppointmentStatus.Cancelled
        };

        _mockAppointmentRepo
            .Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act
        var result = await _sut.CancelAppointmentAsync(appointmentId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Appointment.AlreadyCancelled);
        _mockUoW.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAppointment_WithNonExistent_ShouldReturnNotFound()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();

        _mockAppointmentRepo
            .Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var result = await _sut.CancelAppointmentAsync(appointmentId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Appointment.NotFound);
        _mockUoW.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
