using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Text;

namespace Reaparr.BaseTests;

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

        var bodyResponse = GetEndpointBodyResponse(endpoint);
        if (bodyResponse is not null)
            return bodyResponse;

        throw new InvalidOperationException(
            $"Endpoint '{endpoint.GetType().FullName}' did not store a response of type '{typeof(TResponse).FullName}'."
        );
    }

    private static TResponse? GetEndpointBodyResponse(IEndpoint endpoint)
    {
        var responseBody = endpoint.HttpContext.Response.Body;
        if (!responseBody.CanSeek)
            return null;

        var position = responseBody.Position;
        responseBody.Position = 0;
        using var reader = new StreamReader(responseBody, Encoding.UTF8, leaveOpen: true);
        var body = reader.ReadToEnd();
        responseBody.Position = position;

        if (string.IsNullOrWhiteSpace(body))
            return null;

        if (typeof(TResponse) == typeof(string))
            return (TResponse)(object)body;

        return JsonConvert.DeserializeObject<TResponse>(body);
    }

    private static IValidator? GetEndpointValidator<TRequest>()
        where TRequest : class
    {
        var requestType = typeof(TRequest);

        var validatorTypes = requestType
            .Assembly.GetTypes()
            .Where(t =>
                t is { IsAbstract: false, IsInterface: false }
                && t.GetInterfaces().Any(i => i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IValidator<>)
                    && i.GenericTypeArguments[0] == requestType)
            )
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
                s.AddTransient(_ => IsDatabaseSetup ? IDbContext : Mock.Create<IReaparrDbContext>());
                s.AddTransient(_ => Mock.Mock<IReaparrDbContextFactory>().Object);
                s.AddTransient(_ => IsDatabaseSetup ? IAuthDbContext : Mock.Create<IAuthDbContext>());
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