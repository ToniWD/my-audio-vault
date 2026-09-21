namespace MusicStream.API.Extensions;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MusicStream.Application.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int statusCode;
        string title;
        string detail;
        Dictionary<string, string[]>? validationErrors = null;

        if (exception is FluentValidation.ValidationException fluentException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            title = "Validation Error";
            detail = "One or more validation failures occurred.";

            validationErrors = fluentException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            _logger.LogWarning("FluentValidation failed with {Count} errors.", fluentException.Errors.Count());
        }
        else if (exception is AppException appException)
        {
            statusCode = appException switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest
            };

            title = "Business Exception";
            detail = appException.Message;

            _logger.LogWarning("AppException occurred: {Message}", appException.Message);
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
            title = "Server Error";
            detail = "An unexpected error occurred. Please try again later.";

            _logger.LogError(exception, "Unhandled system exception: {Message}", exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (validationErrors is not null)
        {
            problemDetails.Extensions["errors"] = validationErrors;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}