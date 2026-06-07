using Microsoft.Extensions.DependencyInjection;

namespace Reaparr.BaseTests;

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

        await endpoint.HandleAsync(CancellationToken);

        return new EndpointUnitTestResult<TEndpoint, TResponse>
        {
            Endpoint = endpoint,
            Response = GetEndpointResponse(endpoint),
        };
    }
}

public abstract class BaseEndpointUnitTestBase<TEndpoint, TResponse> : BaseUnitTest<TEndpoint>
    where TEndpoint : class, IEndpoint
    where TResponse : class
{
    protected BaseEndpointUnitTestBase(LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(logEventLevel) { }

    protected static async Task<FluentValidation.Results.ValidationResult?> ValidateEndpointRequestAsync<TRequest>(
        TRequest request,
        CancellationToken cancellationToken
    )
        where TRequest : class
    {
        var validator = GetEndpointValidator<TRequest>();
        if (validator is null)
            return null;

        var context = new FluentValidation.ValidationContext<object>(request);
        return await validator.ValidateAsync(context, cancellationToken);
    }

    protected static TResponse GetEndpointResponse(IEndpoint endpoint)
    {
        if (endpoint.HttpContext.Items.TryGetValue("FastEndpointsResponse", out var response) && response is TResponse typedResponse)
            return typedResponse;

        throw new InvalidOperationException(
            $"Endpoint '{endpoint.GetType().FullName}' did not store a response of type '{typeof(TResponse).FullName}'."
        );
    }

    private static IValidator? GetEndpointValidator<TRequest>()
        where TRequest : class
    {
        var requestType = typeof(TRequest);
        var validatorInterface = typeof(IValidator<>).MakeGenericType(requestType);

        var validatorTypes = requestType
            .Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } && validatorInterface.IsAssignableFrom(t))
            .ToList();

        return validatorTypes.Count switch
        {
            0 => null,
            1 => (IValidator)Activator.CreateInstance(validatorTypes[0])!,
            _ => throw new InvalidOperationException(
                $"Multiple validators found for endpoint request type '{requestType.FullName}'. Endpoint unit tests expect at most one validator."
            ),
        };
    }
}

public sealed class EndpointUnitTestResult<TEndpoint, TResponse>
    where TEndpoint : class, IEndpoint
    where TResponse : class
{
    public required TEndpoint Endpoint { get; init; }

    public required TResponse Response { get; init; }

    public FluentValidation.Results.ValidationResult? ValidationResult { get; init; }

    public bool HasValidator => ValidationResult is not null;

    public bool IsValid => ValidationResult?.IsValid ?? true;
}