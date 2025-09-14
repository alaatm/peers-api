using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Peers.Core.Http;

public static class Result
{
    private static readonly Uri _emptyUri = new(string.Empty, UriKind.RelativeOrAbsolute);

#pragma warning disable CA1054 // URI-like parameters should not be strings
    public static IResult Accepted<TValue>(
        string? uri = null,
        TValue? value = default)
        => Results.Accepted(uri, value);

    public static IResult Accepted(
        string? uri = null,
        object? value = null)
        => Results.Accepted(uri, value);
#pragma warning restore CA1054 // URI-like parameters should not be strings

    public static IResult Created<TValue>(
        Uri? uri = null,
        TValue? value = default)
        => Results.Created(uri ?? _emptyUri, value);

    public static IResult Created(
        Uri? uri = null,
        object? value = null)
        => Results.Created(uri ?? _emptyUri, value);

    public static IResult NoContent()
        => Results.NoContent();

    public static IResult NotFound()
        => Results.NotFound();

    public static IResult File(byte[] fileContents, string contentType, string fileDownloadName)
        => Results.File(fileContents, contentType, fileDownloadName);

    public static IResult ValidationProblem(
        IDictionary<string, string[]> errors,
        string? detail = null,
        string? instance = null,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        IDictionary<string, object?>? extensions = null)
        => Results.ValidationProblem(errors, detail, instance, statusCode, title, type, extensions);

    public static IResult Problem(
        string? detail = null,
        string? instance = null,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        IDictionary<string, object?>? extensions = null)
        => Results.Problem(detail, instance, statusCode, title, type, extensions);

    public static IResult Ok(
        object? value = null) => Results.Ok(value);

    public static IResult Ok<TValue>(
        TValue? value) => Results.Ok(value);

    public static IResult BadRequest(
            string? detail = null,
            string? instance = null,
            string? title = null,
            string? type = null,
            string[]? errors = null) => Results.BadRequest(GetProblemDetails(
                detail: detail,
                instance: instance,
                title: title,
                type: type,
                extensions: errors is null ? null : new Dictionary<string, object?> { { "errors", errors } }));

    public static IResult Conflict(
        string? detail = null,
        string? instance = null,
        string? title = null,
        string? type = null,
        IDictionary<string, object?>? extensions = null) => Results.Conflict(GetProblemDetails(
                detail: detail,
                instance: instance,
                title: title,
                type: type,
                extensions: extensions));

    public static IResult Unauthorized(
        string? detail = null,
        string? instance = null,
        string? title = null,
        string? type = null,
        IDictionary<string, object?>? extensions = null) => new UnauthorizedHttpResult2<ProblemDetails>(GetProblemDetails(
                detail: detail,
                instance: instance,
                title: title,
                type: type,
                extensions: extensions));

    public static IResult Forbidden(
        string? detail = null,
        string? instance = null,
        string? title = null,
        string? type = null,
        IDictionary<string, object?>? extensions = null) => new ForbiddenHttpResult<ProblemDetails>(GetProblemDetails(
                detail: detail,
                instance: instance,
                title: title,
                type: type,
                extensions: extensions));

    public static IResult AccessRestricted(
        string? detail = null,
        string? instance = null,
        string? title = null,
        IDictionary<string, object?>? extensions = null) => Forbidden(
                detail: detail,
                instance: instance,
                title: title,
                type: "ACCESS_RESTRICTED",
                extensions: extensions);

    private static ProblemDetails GetProblemDetails(
            string? detail = null,
            string? instance = null,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            IDictionary<string, object?>? extensions = null)
    {
        var problemDetails = new ProblemDetails
        {
            Detail = detail,
            Instance = instance,
            Status = statusCode,
            Title = title,
            Type = type,
        };

        if (extensions is not null)
        {
            foreach (var extension in extensions)
            {
                problemDetails.Extensions.Add(extension);
            }
        }

        return problemDetails;
    }
}

/// <summary>
/// An <see cref="IResult"/> that on execution will write an object to the response
/// with Unauthorized (401) status code.
/// </summary>
/// <typeparam name="TValue">The type of object that will be JSON serialized to the response body.</typeparam>
public sealed class UnauthorizedHttpResult2<TValue> : IResult/*, IEndpointMetadataProvider*/, IStatusCodeHttpResult, IValueHttpResult, IValueHttpResult<TValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedHttpResult2{TValue}"/> class with the values.
    /// </summary>
    /// <param name="value">The value to format in the entity body.</param>
    internal UnauthorizedHttpResult2(TValue? value)
    {
        Value = value;
        HttpResultsHelper.ApplyProblemDetailsDefaultsIfNeeded(Value, StatusCode);
    }

    /// <summary>
    /// Gets the object result.
    /// </summary>
    public TValue? Value { get; internal init; }

    object? IValueHttpResult.Value => Value;

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status401Unauthorized"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status401Unauthorized;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        // Creating the logger with a string to preserve the category after the refactoring.
        var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("Microsoft.AspNetCore.Http.Result.UnauthorizedHttpObjectResult");

        HttpResultsHelper.Log.WritingResultAsStatusCode(logger, StatusCode);
        httpContext.Response.StatusCode = StatusCode;

        return HttpResultsHelper.WriteResultAsJsonAsync(
                httpContext,
                logger: logger,
                Value);
    }

    //static void IEndpointMetadataProvider.PopulateMetadata(EndpointMetadataContext context)
    //{
    //    ArgumentNullException.ThrowIfNull(context);

    //    context.EndpointMetadata.Add(new ProducesResponseTypeMetadata2(typeof(TValue), StatusCodes.Status401Unauthorized, "application/json"));
    //}
}

/// <summary>
/// An <see cref="IResult"/> that on execution will write an object to the response
/// with Forbidden (403) status code.
/// </summary>
/// <typeparam name="TValue">The type of object that will be JSON serialized to the response body.</typeparam>
public sealed class ForbiddenHttpResult<TValue> : IResult/*, IEndpointMetadataProvider*/, IStatusCodeHttpResult, IValueHttpResult, IValueHttpResult<TValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenHttpResult{TValue}"/> class with the values.
    /// </summary>
    /// <param name="value">The value to format in the entity body.</param>
    internal ForbiddenHttpResult(TValue? value)
    {
        Value = value;
        HttpResultsHelper.ApplyProblemDetailsDefaultsIfNeeded(Value, StatusCode);
    }

    /// <summary>
    /// Gets the object result.
    /// </summary>
    public TValue? Value { get; internal init; }

    object? IValueHttpResult.Value => Value;

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status403Forbidden"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status403Forbidden;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        // Creating the logger with a string to preserve the category after the refactoring.
        var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("Microsoft.AspNetCore.Http.Result.ForbiddenHttpObjectResult");

        HttpResultsHelper.Log.WritingResultAsStatusCode(logger, StatusCode);
        httpContext.Response.StatusCode = StatusCode;

        return HttpResultsHelper.WriteResultAsJsonAsync(
                httpContext,
                logger: logger,
                Value);
    }

    //static void IEndpointMetadataProvider.PopulateMetadata(EndpointMetadataContext context)
    //{
    //    ArgumentNullException.ThrowIfNull(context);

    //    context.EndpointMetadata.Add(new ProducesResponseTypeMetadata2(typeof(TValue), StatusCodes.Status403Forbidden, "application/json"));
    //}
}

internal static partial class HttpResultsHelper
{
    //internal const string DefaultContentType = "text/plain; charset=utf-8";

    public static Task WriteResultAsJsonAsync<T>(
        HttpContext httpContext,
        ILogger logger,
        T? value,
        string? contentType = null,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        if (value is null)
        {
            return Task.CompletedTask;
        }

        var declaredType = typeof(T);

        Log.WritingResultAsJson(logger, declaredType.Name);

        if (declaredType.IsValueType)
        {
            // In this case the polymorphism is not
            // relevant and we don't need to box.
            return httpContext.Response.WriteAsJsonAsync(
                        value,
                        options: jsonSerializerOptions,
                        contentType: contentType);
        }

        return httpContext.Response.WriteAsJsonAsync<object?>(
            value,
            options: jsonSerializerOptions,
            contentType: contentType);
    }

    public static void ApplyProblemDetailsDefaultsIfNeeded(object? value, int? statusCode)
    {
        if (value is ProblemDetails problemDetails)
        {
            ProblemDetailsDefaults.Apply(problemDetails, statusCode);
        }
    }

    internal static partial class Log
    {
        [LoggerMessage(LogLevel.Information,
            "Setting HTTP status code {StatusCode}.",
            EventName = "WritingResultAsStatusCode")]
        public static partial void WritingResultAsStatusCode(ILogger logger, int statusCode);

        //[LoggerMessage(LogLevel.Information,
        //    "Write content with HTTP Response ContentType of {ContentType}",
        //    EventName = "WritingResultAsContent")]
        //public static partial void WritingResultAsContent(ILogger logger, string contentType);

        [LoggerMessage(LogLevel.Information, "Writing value of type '{Type}' as Json.",
            EventName = "WritingResultAsJson")]
        public static partial void WritingResultAsJson(ILogger logger, string type);

        //[LoggerMessage(LogLevel.Information,
        //    "Sending file with download name '{FileDownloadName}'.",
        //    EventName = "WritingResultAsFileWithNoFileName")]
        //public static partial void WritingResultAsFile(ILogger logger, string fileDownloadName);
    }
}

internal static class ProblemDetailsDefaults
{
    public static readonly Dictionary<int, (string Type, string Title)> Defaults = new()
    {
        [400] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            "Bad Request"
        ),

        [401] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            "Unauthorized"
        ),

        [403] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            "Forbidden"
        ),

        [404] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            "Not Found"
        ),

        [405] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.6",
            "Method Not Allowed"
        ),

        [406] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.7",
            "Not Acceptable"
        ),

        [408] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.9",
            "Request Timeout"
        ),

        [409] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            "Conflict"
        ),

        [412] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.13",
            "Precondition Failed"
        ),

        [415] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.16",
            "Unsupported Media Type"
        ),

        [422] =
        (
            "https://tools.ietf.org/html/rfc4918#section-11.2",
            "Unprocessable Entity"
        ),

        [426] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.22",
            "Upgrade Required"
        ),

        [500] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            "An error occurred while processing your request."
        ),

        [502] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.6.3",
            "Bad Gateway"
        ),

        [503] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.6.4",
            "Service Unavailable"
        ),

        [504] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.6.5",
            "Gateway Timeout"
        ),
    };

    public static void Apply(ProblemDetails problemDetails, int? statusCode)
    {
        // We allow StatusCode to be specified either on ProblemDetails or on the ObjectResult and use it to configure the other.
        // This lets users write <c>return Conflict(new Problem("some description"))</c>
        // or <c>return Problem("some-problem", 422)</c> and have the response have consistent fields.
        if (problemDetails.Status is null)
        {
            if (statusCode is not null)
            {
                problemDetails.Status = statusCode;
            }
            else
            {
                problemDetails.Status = problemDetails is HttpValidationProblemDetails ?
                    StatusCodes.Status400BadRequest :
                    StatusCodes.Status500InternalServerError;
            }
        }

        var status = problemDetails.Status.GetValueOrDefault();
        if (Defaults.TryGetValue(status, out var defaults))
        {
            problemDetails.Title ??= defaults.Title;
            problemDetails.Type ??= defaults.Type;
        }
        else if (problemDetails.Title is null)
        {
            var reasonPhrase = ReasonPhrases.GetReasonPhrase(status);
            if (!string.IsNullOrEmpty(reasonPhrase))
            {
                problemDetails.Title = reasonPhrase;
            }
        }
    }
}
