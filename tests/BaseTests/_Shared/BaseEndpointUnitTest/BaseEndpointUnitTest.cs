using Microsoft.Extensions.DependencyInjection;

namespace Reaparr.BaseTests;

public abstract class BaseEndpointUnitTest<TEndpoint, TRequest>
    : BaseEndpointUnitTestBase<TEndpoint, object>
    where TEndpoint : Endpoint<TRequest>
    where TRequest : class
{
    protected BaseEndpointUnitTest(LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(logEventLevel) { }

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
                Response = null!,
            };
        }

        await endpoint.HandleAsync(request, CancellationToken);

        return new EndpointUnitTestResult<TEndpoint, object>
        {
            Endpoint = endpoint,
            ValidationResult = validationResult,
            Response = null!,
        };
    }
}

public abstract class BaseEndpointUnitTest<TEndpoint, TRequest, TResponse>
    : BaseEndpointUnitTestBase<TEndpoint, TResponse>
    where TEndpoint : Endpoint<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    protected BaseEndpointUnitTest(LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(logEventLevel) { }

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
                Response = null!,
            };
        }

        await endpoint.HandleAsync(request, CancellationToken);

        return new EndpointUnitTestResult<TEndpoint, TResponse>
        {
            Endpoint = endpoint,
            Response = GetEndpointResponse(endpoint),
            ValidationResult = validationResult,
        };
    }
}

public abstract class BaseEndpointWithoutRequestUnitTest<TEndpoint, TResponse>
    : BaseEndpointUnitTestBase<TEndpoint, TResponse>
    where TEndpoint : EndpointWithoutRequest<TResponse>
    where TResponse : class
{
    protected BaseEndpointWithoutRequestUnitTest(LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(logEventLevel) { }

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
            Response = GetEndpointResponse(endpoint),
        };
    }
}