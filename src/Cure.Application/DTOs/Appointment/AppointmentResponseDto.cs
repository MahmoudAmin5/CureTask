using Cure.Domain.Entities;

namespace Cure.Application.DTOs.Appointment;

public sealed record AppointmentResponseDto(
    Guid Id,
    Guid PatientId,
    Guid NurseId,
    DateTime ScheduledAtUtc,
    int DurationMinutes,
    string Location,
    string Status,
    string? Notes,
    DateTime CreatedAtUtc)
{
    public static AppointmentResponseDto Map(Domain.Entities.Appointment appointment) =>
        new(
            appointment.Id,
            appointment.PatientId,
            appointment.NurseId,
            appointment.ScheduledAtUtc,
            appointment.DurationMinutes,
            appointment.Location,
            appointment.Status.ToString(),
            appointment.Notes,
            appointment.CreatedAtUtc);
}
