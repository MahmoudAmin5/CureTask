namespace Cure.Domain.Common;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new ArgumentException("A successful result cannot contain an error.", nameof(error));
        }

        if (!isSuccess && error == Error.None)
        {
            throw new ArgumentException("A failed result must contain an error.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);
}

public class Result<T> : Result
{
    private readonly T? _value;
    private readonly IReadOnlyList<Error> _errors;

    private Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
        _errors = error == Error.None
            ? Array.Empty<Error>()
            : new[] { error };
    }

    private Result(IReadOnlyList<Error> errors)
        : base(false, errors.Count > 0 ? errors[0] : Error.NullValue)
    {
        _value = default;
        _errors = errors;
    }

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                $"Cannot access the value of a failed result. Error: {Error.Code} — {Error.Message}");

    public IReadOnlyList<Error> Errors => _errors;

    public static Result<T> Success(T value) => new(value, true, Error.None);

    public new static Result<T> Failure(Error error) => new(default, false, error);

    public static Result<T> ValidationFailure(Error[] errors) => new(errors);
}
