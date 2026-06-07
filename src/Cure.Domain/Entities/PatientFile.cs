using Cure.Domain.Common;

namespace Cure.Domain.Entities;

public sealed class PatientFile : Entity
{
    public Guid PatientId { get; set; }

    public string FileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public long FileSize { get; set; }

    public Patient Patient { get; set; } = null!;
}
