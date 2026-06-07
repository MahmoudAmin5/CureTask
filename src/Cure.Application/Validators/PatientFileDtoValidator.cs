using Cure.Application.DTOs.Clinical;
using FluentValidation;

namespace Cure.Application.Validators;

public sealed class PatientFileDtoValidator : AbstractValidator<PatientFileDto>
{
    public PatientFileDtoValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("Patient ID is required.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.");

        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("File path is required.");

        RuleFor(x => x.FileSize)
            .GreaterThan(0).WithMessage("File size must be greater than zero.");
    }
}
