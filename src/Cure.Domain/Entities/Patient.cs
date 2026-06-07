using Cure.Domain.Common;

namespace Cure.Domain.Entities;

public sealed class Patient : Entity
{
    public string UserId { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = null!;

    public string? BloodType { get; set; }

    public string Address { get; set; } = null!;

    public string? EmergencyContact { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();

    public ICollection<ClinicalNote> ClinicalNotes { get; set; } = new List<ClinicalNote>();

    public ICollection<PatientFile> PatientFiles { get; set; } = new List<PatientFile>();
}
