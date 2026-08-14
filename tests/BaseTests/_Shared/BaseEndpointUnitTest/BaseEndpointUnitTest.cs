using Microsoft.Extensions.DependencyInjection;

namespace Reaparr.BaseTests;

/// <summary>
/// Base class for endpoint unit tests whose endpoint accepts a request but does not declare a typed response DTO.
/// </summary>
/// <typeparam name="TEndpoint">The endpoint type under test.</typeparam>
/// <typeparam name="TRequest">The request DTO type accepted by the endpoint.</typeparam>
public abstract class BaseEndpointUnitTest<TEndpoint, TRequest> : BaseEndpointUnitTestBase<TEndpoint, object>
    where TEndpoint : Endpoint<TRequest>
    where TRequest : class
{
    protected BaseEndpointUnitTest(LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(logEventLevel) { }

    /// <summary>
    /// Validates the request and invokes the endpoint's <c>HandleAsync</c> method only when validation succeeds.
    /// </summary>
    /// <param name="request">The request DTO passed to the endpoint.</param>
    /// <param name="extraServices">Optional test-specific service registrations.</param>
    /// <returns>The endpoint, validation outcome, HTTP status metadata, and captured response body.</returns>
    protected async Task<EndpointUnitTestResult<TEndpoint, object>> TestEndpointHandleAsync(
        TRequest request,
        Action<IServiceCollection>? extraServices = null
    )
    {
        var endpoint = SetupEndpointUnitTest<TEndpoint>(extraServices);

        endpoint.HttpContext.Response.Body = new MemoryStream();

        var validationResult = await ValidateEndpointRequestAsync(request, CancellationToken);

        if (validationResult is { IsValid: false })
        {
            return new EndpointUnitTestResult<TEndpoint, object>
            {
                Endpoint = endpoint,
                ValidationResult = validationResult,
            };
        }

        await endpoint.HandleAsync(request, CancellationToken);

        return new EndpointUnitTestResult<TEndpoint, object>
        {
            Endpoint = endpoint,
            Response = await GetEndpointResponseAsync(endpoint, CancellationToken),
            ValidationResult = validationResult,
        };
    }
}

/// <summary>
/// Base class for endpoint unit tests whose endpoint accepts a request and declares a typed response DTO.
/// </summary>
/// <typeparam name="TEndpoint">The endpoint type under test.</typeparam>
/// <typeparam name="TRequest">The request DTO type accepted by the endpoint.</typeparam>
/// <typeparam name="TResponse">The response DTO type expected from the endpoint.</typeparam>
public abstract class BaseEndpointUnitTest<TEndpoint, TRequest, TResponse>
    : BaseEndpointUnitTestBase<TEndpoint, TResponse>
    where TEndpoint : Endpoint<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    protected BaseEndpointUnitTest(LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(logEventLevel) { }

    /// <summary>
    /// Validates the request and invokes the endpoint's <c>HandleAsync</c> method only when validation succeeds.
    /// </summary>
    /// <param name="request">The request DTO passed to the endpoint.</param>
    /// <param name="extraServices">Optional test-specific service registrations.</param>
    /// <returns>The endpoint, validation outcome, HTTP status metadata, and captured response DTO.</returns>
    protected async Task<EndpointUnitTestResult<TEndpoint, TResponse>> TestEndpointHandleAsync(
        TRequest request,
        Action<IServiceCollection>? extraServices = null
    )
    {
        var endpoint = SetupEndpointUnitTest<TEndpoint>(extraServices);
        endpoint.HttpContext.Response.Body = new MemoryStream();
        var validationResult = await ValidateEndpointRequestAsync(request, CancellationToken);

        if (validationResult is { IsValid: false })
        {
            return new EndpointUnitTestResult<TEndpoint, TResponse>
            {
                Endpoint = endpoint,
                ValidationResult = validationResult,
            };
        }

        await endpoint.HandleAsync(request, CancellationToken);

        return new EndpointUnitTestResult<TEndpoint, TResponse>
        {
            Endpoint = endpoint,
            Response = await GetEndpointResponseAsync(endpoint, CancellationToken),
            ValidationResult = validationResult,
        };
    }
}

/// <summary>
/// Base class for endpoint unit tests whose endpoint has no request DTO and declares a typed response DTO.
/// </summary>
/// <typeparam name="TEndpoint">The endpoint type under test.</typeparam>
/// <typeparam name="TResponse">The response DTO type expected from the endpoint.</typeparam>
public abstract class BaseEndpointWithoutRequestUnitTest<TEndpoint, TResponse>
    : BaseEndpointUnitTestBase<TEndpoint, TResponse>
    where TEndpoint : EndpointWithoutRequest<TResponse>
    where TResponse : class
{
    protected BaseEndpointWithoutRequestUnitTest(LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(logEventLevel) { }

    /// <summary>
    /// Invokes the endpoint's <c>HandleAsync</c> method using the endpoint test service container.
    /// </summary>
    /// <param name="extraServices">Optional test-specific service registrations.</param>
    /// <returns>The endpoint, HTTP status metadata, and captured response DTO.</returns>
    protected async Task<EndpointUnitTestResult<TEndpoint, TResponse>> TestEndpointHandleAsync(
        Action<IServiceCollection>? extraServices = null
    )
    {
        var endpoint = SetupEndpointUnitTest<TEndpoint>(extraServices);

        endpoint.HttpContext.Response.Body = new MemoryStream();

        await endpoint.HandleAsync(CancellationToken);

        return new EndpointUnitTestResult<TEndpoint, TResponse>
        {
            Endpoint = endpoint,
            Response = await GetEndpointResponseAsync(endpoint, CancellationToken),
        };
    }
}
