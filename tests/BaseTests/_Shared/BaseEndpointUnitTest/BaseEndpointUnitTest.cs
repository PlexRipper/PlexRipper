using Microsoft.Extensions.DependencyInjection;

namespace Reaparr.BaseTests;

public abstract class BaseEndpointUnitTest<TEndpoint, TRequest, TResponse> : BaseUnitTest<TEndpoint>
    where TEndpoint : Application.BaseEndpoint<TRequest>
    where TRequest : class
    where TResponse : BaseResultDTO
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
            };
        }

        await endpoint.HandleAsync(request, CancellationToken);

        return new EndpointUnitTestResult<TEndpoint, TResponse>
        {
            Endpoint = endpoint,
            Result = GetEndpointResponse(endpoint),
            ValidationResult = validationResult,
        };
    }

    protected static async Task<FluentValidation.Results.ValidationResult?> ValidateEndpointRequestAsync(
        TRequest request,
        CancellationToken cancellationToken
    )
    {
        IValidator? validator;
        var requestType = typeof(TRequest);
        var validatorInterface = typeof(IValidator<>).MakeGenericType(requestType);

        var validatorTypes = requestType
            .Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } && validatorInterface.IsAssignableFrom(t))
            .ToList();

        if (validatorTypes.Count == 0)
            validator = null;
        else
        {
            if (validatorTypes.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Multiple validators found for endpoint request type '{requestType.FullName}'. Endpoint unit tests expect at most one validator."
                );
            }

            validator = (IValidator)Activator.CreateInstance(validatorTypes.First())!;
        }

        if (validator is null)
            return null;

        var context = new FluentValidation.ValidationContext<object>(request);
        return await validator.ValidateAsync(context, cancellationToken);
    }

    protected static TResponse GetEndpointResponse(IEndpoint endpoint)
    {
        var endpointResponseProperty = endpoint.GetType().GetProperty(nameof(Endpoint<>.Response));
        var response = endpointResponseProperty?.GetValue(endpoint);

        return response as TResponse
               ?? throw new InvalidOperationException(
                   $"Endpoint '{endpoint.GetType().FullName}' returned response type '{response?.GetType().FullName ?? "null"}', expected '{typeof(TResponse).FullName}'."
               );
    }
}

public abstract class BaseEndpointWithoutRequestUnitTest<TEndpoint, TResponse> : BaseUnitTest<TEndpoint>
    where TEndpoint : Application.BaseEndpointWithoutRequest
    where TResponse : BaseResultDTO
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
            Result = GetEndpointResponse(endpoint),
        };
    }

    protected static TResponse GetEndpointResponse(IEndpoint endpoint)
    {
        var endpointResponseProperty = endpoint.GetType().GetProperty(nameof(Endpoint<>.Response));
        var response = endpointResponseProperty?.GetValue(endpoint);

        return response as TResponse
               ?? throw new InvalidOperationException(
                   $"Endpoint '{endpoint.GetType().FullName}' returned response type '{response?.GetType().FullName ?? "null"}', expected '{typeof(TResponse).FullName}'."
               );
    }

    protected static async Task<FluentValidation.Results.ValidationResult?> ValidateEndpointRequestAsync<TRequest>(
        TRequest request,
        CancellationToken cancellationToken
    )
        where TRequest : class
    {
        IValidator? validator;
        var requestType = typeof(TRequest);
        var validatorInterface = typeof(IValidator<>).MakeGenericType(requestType);

        var validatorTypes = requestType
            .Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } && validatorInterface.IsAssignableFrom(t))
            .ToList();

        if (validatorTypes.Count == 0)
            validator = null;
        else
        {
            if (validatorTypes.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Multiple validators found for endpoint request type '{requestType.FullName}'. Endpoint unit tests expect at most one validator."
                );
            }

            validator = (IValidator)Activator.CreateInstance(validatorTypes.First())!;
        }

        if (validator is null)
            return null;

        var context = new FluentValidation.ValidationContext<object>(request);
        return await validator.ValidateAsync(context, cancellationToken);
    }
}

public sealed class EndpointUnitTestResult<TEndpoint, TResponse>
    where TEndpoint : class, IEndpoint
    where TResponse : BaseResultDTO
{
    public required TEndpoint Endpoint { get; init; }

    public TResponse? Result { get; init; }

    public FluentValidation.Results.ValidationResult? ValidationResult { get; init; }

    public bool HasValidator => ValidationResult is not null;

    public bool IsValid => ValidationResult?.IsValid ?? true;
}