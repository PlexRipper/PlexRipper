using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        if (endpoint.HttpContext.Items.TryGetValue("FastEndpointsResponse", out var response) &&
            response is TResponse typedResponse)
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

    protected T SetupEndpointUnitTest<T>(Action<IServiceCollection>? extraServices = null)
        where T : class, IEndpoint
    {
        return Factory.Create<T>(ctx =>
        {
            ctx.AddTestServices(s =>
            {
                // All different dependencies that are needed for the endpoint need to be added here. And then they can be mocked in the test.
                s.AddTransient(_ => Mock.Create<ILogger>());
                s.AddTransient(_ => Mock.Create<IReaparrDbContext>());
                s.AddTransient(_ => Mock.Mock<IReaparrDbContextFactory>().Object);
                s.AddTransient(_ => Mock.Create<IAuthDbContext>());
                s.AddTransient(_ => Mock.Create<IAuthDbContextFactory>());
                s.AddTransient(_ => Mock.Mock<ICommandExecutor>().Object);
                s.AddSingleton(_ => Mock.Create<ISchedulerService>());
                s.AddSingleton(_ => Mock.Mock<IProgressHubService>().Object);
                s.AddSingleton(_ => Mock.Mock<IDownloadHubService>().Object);
                s.AddSingleton(_ => Mock.Mock<INotificationHubService>().Object);
                s.AddSingleton(_ => Mock.Mock<IDownloadTaskScheduler>().Object);
                s.AddSingleton(_ => Mock.Container.Resolve<IPathProvider>());
                s.AddSingleton(_ => Mock.Container.Resolve<IAppBuildInfo>());
                s.AddSingleton(_ => Mock.Mock<IHostApplicationLifetime>().Object);

                extraServices?.Invoke(s);
            });
        });
    }
}