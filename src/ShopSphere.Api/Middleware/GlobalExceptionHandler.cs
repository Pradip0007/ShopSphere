using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ShopSphere.Api.Middleware;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        (int statusCode, string title, IDictionary<string, object?>? extensions) = MapException(exception);

        httpContext.Response.StatusCode = statusCode;

        logger.Log(
            statusCode >= 500 ? LogLevel.Error : LogLevel.Warning,
            exception,
            "Request failed: {StatusCode} {Title}",
            statusCode,
            title);

        ProblemDetails problemDetails = new()
        {
            Type = $"https://httpstatuses.io/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = statusCode >= 500 ? "An unexpected error occurred." : exception.Message,
            Instance = httpContext.Request.Path,
        };

        if (extensions is not null)
        {
            foreach ((string key, object? value) in extensions)
            {
                problemDetails.Extensions[key] = value;
            }
        }

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails,
        });
    }

    private static (int StatusCode, string Title, IDictionary<string, object?>? Extensions) MapException(
    Exception exception) => exception switch
{
    ValidationException validation => (
        (int)HttpStatusCode.BadRequest,
        "One or more validation errors occurred.",
        new Dictionary<string, object?>
        {
            ["errors"] = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => (object?)g.Select(e => e.ErrorMessage).ToArray()),
        }),

    KeyNotFoundException => (
        (int)HttpStatusCode.NotFound,
        "Resource not found.",
        null),

    // 👇 Add here
    ConflictException => (
        (int)HttpStatusCode.Conflict,
        "Resource conflict.",
        null),

    // 👇 Add here
    BusinessRuleException businessRule => (
        (int)HttpStatusCode.UnprocessableEntity,
        "Business rule violation.",
        new Dictionary<string, object?>
        {
            ["rule"] = businessRule.Message
        }),

    UnauthorizedAccessException => (
        (int)HttpStatusCode.Unauthorized,
        "Authentication required.",
        null),

    _ => (
        (int)HttpStatusCode.InternalServerError,
        "An unexpected error occurred.",
        null),
    };
}