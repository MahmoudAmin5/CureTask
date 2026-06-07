using Cure.Domain.Common;
using Cure.Domain.Entities;

namespace Cure.Domain.Services;

public interface IAppointmentService
{
    Task<Result<Appointment>> BookAppointmentAsync(
        Guid patientId,
        Guid nurseId,
        DateTime scheduledAtUtc,
        int durationMinutes,
        string location,
        string? notes,
        CancellationToken cancellationToken = default);

    Task<Result<Appointment>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<Appointment>>> GetByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<Appointment>>> GetByNurseIdAsync(
        Guid nurseId,
        CancellationToken cancellationToken = default);

    Task<Result> CancelAppointmentAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<Appointment>>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
