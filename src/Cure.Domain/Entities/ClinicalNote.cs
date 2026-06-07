using Cure.Domain.Common;

namespace Cure.Domain.Entities;

public sealed class ClinicalNote : Entity
{
    public Guid PatientId { get; set; }

    public string AuthorId { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string NoteType { get; set; } = null!;

    public Patient Patient { get; set; } = null!;
}
