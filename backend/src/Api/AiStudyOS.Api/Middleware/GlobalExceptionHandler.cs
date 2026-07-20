using AiStudyOS.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AiStudyOS.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, extensions) = Map(exception);

        if (statusCode >= 500)
            logger.LogError(exception, "Unhandled exception");

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.io/{statusCode}",
        };

        if (extensions is not null)
            foreach (var (key, value) in extensions)
                problemDetails.Extensions[key] = value;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, Dictionary<string, object?>? Extensions) Map(Exception exception) => exception switch
    {
        FluentValidation.ValidationException validationException => (
            StatusCodes.Status400BadRequest,
            "One or more validation errors occurred.",
            new Dictionary<string, object?>
            {
                ["errors"] = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
            }),
        EmailAlreadyExistsException ex => (StatusCodes.Status409Conflict, ex.Message, null),
        InvalidCredentialsException ex => (StatusCodes.Status401Unauthorized, ex.Message, null),
        InvalidRefreshTokenException ex => (StatusCodes.Status401Unauthorized, ex.Message, null),
        NotFoundException ex => (StatusCodes.Status404NotFound, ex.Message, null),
        AiGenerationFailedException ex => (StatusCodes.Status503ServiceUnavailable, ex.Message, null),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", null),
    };
}
