using Cure.Application.DTOs.Appointment;
using FluentValidation;

namespace Cure.Application.Validators;

public sealed class BookAppointmentDtoValidator : AbstractValidator<BookAppointmentDto>
{
    public BookAppointmentDtoValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("Patient ID is required.");

        RuleFor(x => x.NurseId)
            .NotEmpty().WithMessage("Nurse ID is required.");

        RuleFor(x => x.ScheduledAtUtc)
            .GreaterThan(DateTime.UtcNow).WithMessage("Scheduled date must be in the future.");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(15, 480).WithMessage("Duration must be between 15 and 480 minutes.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");
    }
}
