using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

    private static IValidator? GetEndpointValidator<TRequest>()
        where TRequest : class
    {
        var requestType = typeof(TRequest);

        var validatorTypes = requestType
            .Assembly.GetTypes()
            .Where(t =>
                t is { IsAbstract: false, IsInterface: false }
                && t.GetInterfaces()
                    .Any(i => i.IsGenericType
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

    protected static async Task<TResponse?> GetEndpointResponseAsync(TEndpoint endpoint, CancellationToken cancellationToken)
    {
        try
        {
            var responseProperty = endpoint.GetType().GetProperty(nameof(Endpoint<,>.Response));
            if (responseProperty?.GetValue(endpoint) is TResponse response)
                return response;
        }
        catch (Exception ex) when (ex is NotSupportedException or TargetInvocationException { InnerException: NotSupportedException })
        {
            // Some FastEndpoints response DTOs cannot be auto-created because they have required members.
            // Those endpoints still write their response to the HTTP body via SendAsync/Send.FluentResult.
        }

        if (endpoint.HttpContext.Response.Body is not MemoryStream body || body.Length == 0)
            return null;

        if (typeof(TResponse) == typeof(string))
        {
            body.Position = 0;
            using var reader = new StreamReader(body, Encoding.UTF8, leaveOpen: true);
            var text = await reader.ReadToEndAsync(cancellationToken);
            body.Position = 0;
            return text as TResponse;
        }

        try
        {
            body.Position = 0;
            var response = await JsonSerializer.DeserializeAsync<TResponse>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard,
                cancellationToken
            );
            body.Position = 0;
            return response;
        }
        catch (JsonException)
        {
            body.Position = 0;
            return null;
        }
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