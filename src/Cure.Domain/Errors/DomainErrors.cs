using Cure.Domain.Common;

namespace Cure.Domain.Errors;

public static class DomainErrors
{
    public static class General
    {
        public static Error NotFound(string entityName, Guid id) =>
            Error.NotFound(
                "General.NotFound",
                $"The {entityName} with Id '{id}' was not found.");

        public static readonly Error ServerError = new(
            "General.ServerError",
            "An unexpected error occurred on the server.");

        public static readonly Error UnprocessableEntity = new(
            "General.UnprocessableEntity",
            "The request could not be processed.");
    }

    public static class Appointment
    {
        public static readonly Error InvalidTime = Error.Validation(
            "Appointment.InvalidTime",
            "The specified appointment time is invalid.");

        public static readonly Error DoubleBookingConflict = Error.Conflict(
            "Appointment.DoubleBookingConflict",
            "The nurse already has an appointment scheduled during the requested time slot.");

        public static readonly Error NotFound = Error.NotFound(
            "Appointment.NotFound",
            "The specified appointment was not found.");

        public static readonly Error AlreadyCancelled = Error.Validation(
            "Appointment.AlreadyCancelled",
            "The appointment has already been cancelled.");

        public static readonly Error PastDateNotAllowed = Error.Validation(
            "Appointment.PastDateNotAllowed",
            "Appointments cannot be scheduled for a past date.");

        public static readonly Error ConcurrencyConflict = Error.Conflict(
            "Appointment.ConcurrencyConflict",
            "The appointment was modified by another request. Please retry.");
    }

    public static class Authentication
    {
        public static readonly Error InvalidCredentials = Error.Unauthorized(
            "Authentication.InvalidCredentials",
            "The provided email or password is incorrect.");

        public static readonly Error UserAlreadyExists = Error.Conflict(
            "Authentication.UserAlreadyExists",
            "A user with this email address already exists.");

        public static readonly Error InvalidRefreshToken = Error.Unauthorized(
            "Authentication.InvalidRefreshToken",
            "The provided refresh token is invalid.");

        public static readonly Error RefreshTokenExpired = Error.Unauthorized(
            "Authentication.RefreshTokenExpired",
            "The refresh token has expired.");
    }

    public static class Patient
    {
        public static readonly Error NotFound = Error.NotFound(
            "Patient.NotFound",
            "The specified patient was not found.");

        public static readonly Error AlreadyExists = Error.Conflict(
            "Patient.AlreadyExists",
            "A patient with the specified details already exists.");
    }

    public static class Nurse
    {
        public static readonly Error NotFound = Error.NotFound(
            "Nurse.NotFound",
            "The specified nurse was not found.");

        public static readonly Error AlreadyExists = Error.Conflict(
            "Nurse.AlreadyExists",
            "A nurse with the specified details already exists.");
    }

    public static class Validation
    {
        public static readonly Error General = Error.Validation(
            "Validation.General",
            "One or more validation errors occurred.");
    }
}
