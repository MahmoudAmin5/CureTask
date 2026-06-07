using System.ComponentModel.DataAnnotations;
using Cure.Domain.Common;
using Cure.Domain.Enums;

namespace Cure.Domain.Entities;

public sealed class Appointment : Entity
{
    public Guid PatientId { get; set; }

    public Guid NurseId { get; set; }

    public DateTime ScheduledAtUtc { get; set; }

    public int DurationMinutes { get; set; }

    public string Location { get; set; } = null!;

    public AppointmentStatus Status { get; set; }

    public string? Notes { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    public Patient Patient { get; set; } = null!;

    public Nurse Nurse { get; set; } = null!;
}
