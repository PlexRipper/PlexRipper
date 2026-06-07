using FluentValidation.Results;

namespace Reaparr.BaseTests;

public sealed record EndpointUnitTestResult<TEndpoint, TResponse>
    where TEndpoint : class, IEndpoint
    where TResponse : class
{
    public required TEndpoint Endpoint { get; init; }

    public TResponse? Response => Endpoint.GetType().GetProperty(nameof(Response))?.GetValue(Endpoint) as TResponse;

    public FluentValidation.Results.ValidationResult? ValidationResult { get; init; }

    public IReadOnlyList<ValidationFailure> ValidationErrors => [.. (ValidationResult?.Errors ?? []), ..Endpoint.ValidationFailures];

    public bool HasValidator => ValidationResult is not null;

    public bool IsValid => (ValidationResult?.IsValid ?? true) && ValidationErrors.Count == 0;

    public int StatusCode => Endpoint.HttpContext.Response.StatusCode;  
    
    public string ContentType => Endpoint.HttpContext.Response.ContentType ?? string.Empty;
}