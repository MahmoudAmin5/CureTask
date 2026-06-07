namespace Cure.Application.DTOs.Clinical;

public sealed record PatientFileDto(
    Guid? Id,
    Guid PatientId,
    string FileName,
    string ContentType,
    string FilePath,
    long FileSize);
