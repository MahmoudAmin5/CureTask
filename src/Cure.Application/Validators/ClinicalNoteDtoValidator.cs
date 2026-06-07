using Cure.Application.DTOs.Clinical;
using FluentValidation;

namespace Cure.Application.Validators;

public sealed class ClinicalNoteDtoValidator : AbstractValidator<ClinicalNoteDto>
{
    public ClinicalNoteDtoValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("Patient ID is required.");

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("Author ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(5000).WithMessage("Content must not exceed 5000 characters.");

        RuleFor(x => x.NoteType)
            .NotEmpty().WithMessage("Note type is required.")
            .MaximumLength(100).WithMessage("Note type must not exceed 100 characters.");
    }
}
