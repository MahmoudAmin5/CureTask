using Cure.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Cure.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        return MapFailure(result.Error, result.Errors);
    }

    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return new NoContentResult();
        }

        return MapFailure(result.Error);
    }

    private static IActionResult MapFailure(Error error, IReadOnlyList<Error>? errors = null)
    {
        // Multiple validation errors
        if (errors is not null && errors.Count > 1)
        {
            var validationProblem = new ValidationProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7807",
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred."
            };

            foreach (var err in errors)
            {
                if (validationProblem.Errors.ContainsKey(err.Code))
                {
                    var existing = validationProblem.Errors[err.Code];
                    validationProblem.Errors[err.Code] = [.. existing, err.Message];
                }
                else
                {
                    validationProblem.Errors[err.Code] = [err.Message];
                }
            }

            return new BadRequestObjectResult(validationProblem);
        }

        var code = error.Code;

        if (code.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
        {
            return new NotFoundObjectResult(CreateProblemDetails(
                "Not Found",
                StatusCodes.Status404NotFound,
                error));
        }

        if (code.Contains("Conflict", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("DoubleBooking", StringComparison.OrdinalIgnoreCase))
        {
            return new ConflictObjectResult(CreateProblemDetails(
                "Conflict",
                StatusCodes.Status409Conflict,
                error));
        }

        if (code.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("InvalidCredentials", StringComparison.OrdinalIgnoreCase))
        {
            return new ObjectResult(CreateProblemDetails(
                "Unauthorized",
                StatusCodes.Status401Unauthorized,
                error))
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        return new BadRequestObjectResult(CreateProblemDetails(
            "Bad Request",
            StatusCodes.Status400BadRequest,
            error));
    }

    private static ProblemDetails CreateProblemDetails(
        string title,
        int status,
        Error error)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Title = title,
            Status = status,
            Detail = error.Message,
            Extensions =
            {
                ["errorCode"] = error.Code
            }
        };
    }
}
