namespace Cure.Domain.Common;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    public static Error Validation(string code, string message) => new(code, message);

    public static Error NotFound(string code, string message) => new(code, message);

    public static Error Conflict(string code, string message) => new(code, message);

    public static Error Unauthorized(string code, string message) => new(code, message);
}
