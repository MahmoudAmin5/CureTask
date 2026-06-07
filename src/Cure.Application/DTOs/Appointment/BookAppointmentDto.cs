namespace Cure.Application.DTOs.Appointment;

public sealed record BookAppointmentDto(
    Guid PatientId,
    Guid NurseId,
    DateTime ScheduledAtUtc,
    int DurationMinutes,
    string Location,
    string? Notes);
