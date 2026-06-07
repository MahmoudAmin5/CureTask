namespace Cure.Application.DTOs.Clinical;

public sealed record ClinicalNoteDto(
    Guid? Id,
    Guid PatientId,
    Guid AuthorId,
    string Content,
    string NoteType);
