using Cure.Domain.Common;

namespace Cure.Domain.Entities;

public sealed class MedicalHistory : Entity
{
    public Guid PatientId { get; set; }

    public string Condition { get; set; } = null!;

    public DateTime DiagnosedDate { get; set; }

    public string? Treatment { get; set; }

    public bool IsCurrent { get; set; }

    public Patient Patient { get; set; } = null!;
}
