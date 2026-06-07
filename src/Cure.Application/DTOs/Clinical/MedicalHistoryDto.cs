namespace Cure.Application.DTOs.Clinical;

public sealed record MedicalHistoryDto(
    Guid? Id,
    Guid PatientId,
    string Condition,
    DateTime DiagnosedDate,
    string Treatment,
    bool IsCurrent);
