using Cure.Application.DTOs.Clinical;
using FluentValidation;

namespace Cure.Application.Validators;

public sealed class MedicalHistoryDtoValidator : AbstractValidator<MedicalHistoryDto>
{
    public MedicalHistoryDtoValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("Patient ID is required.");

        RuleFor(x => x.Condition)
            .NotEmpty().WithMessage("Condition is required.")
            .MaximumLength(500).WithMessage("Condition must not exceed 500 characters.");

        RuleFor(x => x.Treatment)
            .MaximumLength(1000).WithMessage("Treatment must not exceed 1000 characters.");
    }
}
