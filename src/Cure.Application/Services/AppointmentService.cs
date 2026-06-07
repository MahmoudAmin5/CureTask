using Cure.Application.Abstractions;
using Cure.Application.DTOs.Appointment;
using Cure.Domain.Common;
using Cure.Domain.Entities;
using Cure.Domain.Enums;
using Cure.Domain.Errors;
using Cure.Domain.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cure.Application.Services;

public sealed class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<BookAppointmentDto> _validator;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IUnitOfWork unitOfWork,
        IValidator<BookAppointmentDto> validator,
        ILogger<AppointmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Appointment>> BookAppointmentAsync(
        Guid patientId,
        Guid nurseId,
        DateTime scheduledAtUtc,
        int durationMinutes,
        string location,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var dto = new BookAppointmentDto(patientId, nurseId, scheduledAtUtc, durationMinutes, location, notes);

        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                .ToArray();

            _logger.LogWarning("Appointment booking validation failed with {ErrorCount} error(s)", errors.Length);
            return Result<Appointment>.ValidationFailure(errors);
        }

        if (scheduledAtUtc <= DateTime.UtcNow)
        {
            return Result<Appointment>.Failure(DomainErrors.Appointment.PastDateNotAllowed);
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var repo = _unitOfWork.Repository<Appointment>();

            var overlapping = await repo.FindAsync(a =>
                a.NurseId == nurseId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.ScheduledAtUtc < scheduledAtUtc.AddMinutes(durationMinutes) &&
                scheduledAtUtc < a.ScheduledAtUtc.AddMinutes(a.DurationMinutes),
                cancellationToken);

            if (overlapping.Any())
            {
                _logger.LogWarning(
                    "Double booking conflict detected for nurse {NurseId} at {ScheduledAtUtc}",
                    nurseId,
                    scheduledAtUtc);

                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Appointment>.Failure(DomainErrors.Appointment.DoubleBookingConflict);
            }

            
            var patient = await _unitOfWork.Repository<Patient>().GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Appointment>.Failure(DomainErrors.Patient.NotFound);
            }

            
            var nurse = await _unitOfWork.Repository<Nurse>().GetByIdAsync(nurseId, cancellationToken);
            if (nurse is null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Appointment>.Failure(DomainErrors.Nurse.NotFound);
            }

            var appointment = new Appointment
            {
                Id = Guid.CreateVersion7(),
                PatientId = patientId,
                NurseId = nurseId,
                ScheduledAtUtc = scheduledAtUtc,
                DurationMinutes = durationMinutes,
                Location = location,
                Notes = notes,
                Status = AppointmentStatus.Pending
            };

            repo.Add(appointment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Appointment {AppointmentId} booked successfully", appointment.Id);
            return Result<Appointment>.Success(appointment);
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogWarning(
                "Concurrency conflict detected while booking appointment for nurse {NurseId}",
                nurseId);

            return Result<Appointment>.Failure(DomainErrors.Appointment.DoubleBookingConflict);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogError(ex, "Unexpected error booking appointment for patient {PatientId}", patientId);
            throw;
        }
    }

    public async Task<Result<Appointment>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _unitOfWork.Repository<Appointment>()
            .GetByIdAsync(id, cancellationToken);

        if (appointment is null)
        {
            return Result<Appointment>.Failure(DomainErrors.Appointment.NotFound);
        }

        return Result<Appointment>.Success(appointment);
    }

    public async Task<Result<IReadOnlyList<Appointment>>> GetByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var appointments = await _unitOfWork.Repository<Appointment>()
            .FindAsync(a => a.PatientId == patientId, cancellationToken);

        return Result<IReadOnlyList<Appointment>>.Success(appointments);
    }

    public async Task<Result<IReadOnlyList<Appointment>>> GetByNurseIdAsync(
        Guid nurseId,
        CancellationToken cancellationToken = default)
    {
        var appointments = await _unitOfWork.Repository<Appointment>()
            .FindAsync(a => a.NurseId == nurseId, cancellationToken);

        return Result<IReadOnlyList<Appointment>>.Success(appointments);
    }

    public async Task<Result> CancelAppointmentAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var repo = _unitOfWork.Repository<Appointment>();
            var appointment = await repo.GetByIdAsync(id, cancellationToken);

            if (appointment is null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure(DomainErrors.Appointment.NotFound);
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure(DomainErrors.Appointment.AlreadyCancelled);
            }

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAtUtc = DateTime.UtcNow;
            repo.Update(appointment);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Appointment {AppointmentId} cancelled successfully", id);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogWarning(
                "Concurrency conflict detected while cancelling appointment {AppointmentId}",
                id);

            return Result.Failure(DomainErrors.Appointment.ConcurrencyConflict);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogError(ex, "Unexpected error cancelling appointment {AppointmentId}", id);
            throw;
        }
    }

    public async Task<Result<PagedResult<Appointment>>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Appointment>();

        var items = await repo.GetPagedAsync(null, page, pageSize, cancellationToken);
        var totalCount = await repo.CountAsync(cancellationToken: cancellationToken);

        var pagedResult = new PagedResult<Appointment>(items, totalCount, page, pageSize);
        return Result<PagedResult<Appointment>>.Success(pagedResult);
    }
}
