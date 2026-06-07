using FluentValidation.Results;

namespace Reaparr.BaseTests;

/// <summary>
/// Captures the observable result of invoking a FastEndpoints endpoint in a unit test.
/// </summary>
/// <remarks>
/// The response is intentionally stored as a value captured by the test helper instead of being
/// read lazily from <see cref="IEndpoint"/>. FastEndpoints may throw while auto-creating response
/// DTOs that contain required members; capturing once during helper execution keeps the failure
/// location near the endpoint invocation and avoids reflection wrapping in assertion code.
/// </remarks>
/// <typeparam name="TEndpoint">The endpoint type under test.</typeparam>
/// <typeparam name="TResponse">The expected response DTO type for the endpoint.</typeparam>
public sealed record EndpointUnitTestResult<TEndpoint, TResponse>
    where TEndpoint : class, IEndpoint
    where TResponse : class
{
    /// <summary>
    /// Gets the configured endpoint instance after validation and optional execution.
    /// </summary>
    public required TEndpoint Endpoint { get; init; }

    /// <summary>
    /// Gets the response captured by the endpoint test helper, or <see langword="null"/> when
    /// validation prevented execution or the endpoint intentionally wrote a non-DTO response.
    /// </summary>
    public TResponse? Response { get; init; }

    /// <summary>
    /// Gets the FluentValidation result produced before endpoint execution, when a validator exists.
    /// </summary>
    public FluentValidation.Results.ValidationResult? ValidationResult { get; init; }

    /// <summary>
    /// Gets both standalone validator failures and failures collected by FastEndpoints itself.
    /// </summary>
    public IReadOnlyList<ValidationFailure> ValidationErrors => [.. (ValidationResult?.Errors ?? []), ..Endpoint.ValidationFailures];

    /// <summary>
    /// Gets whether the request type had a validator available to the endpoint unit test helper.
    /// </summary>
    public bool HasValidator => ValidationResult is not null;

    /// <summary>
    /// Gets whether request validation passed and FastEndpoints has no additional validation failures.
    /// </summary>
    public bool IsValid => (ValidationResult?.IsValid ?? true) && ValidationErrors.Count == 0;

    /// <summary>
    /// Gets the HTTP status code written by the endpoint.
    /// </summary>
    public int StatusCode => Endpoint.HttpContext.Response.StatusCode;

    /// <summary>
    /// Gets the HTTP content type written by the endpoint, or an empty string when no type was set.
    /// </summary>
    public string ContentType => Endpoint.HttpContext.Response.ContentType ?? string.Empty;
}