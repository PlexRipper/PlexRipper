using System.Reflection;
using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Environment;
using Reaparr.Identity.Contracts;
using Serilog.Events;

namespace Reaparr.BaseTests;

public partial class BaseUnitTest
{
    protected readonly ITestOutputHelper Output;
    protected readonly LogEventLevel LogEventLevel;

    protected readonly ILogger Log;

    // Use loose behavior here to avoid Dispose() not mocked exception
    protected Mock<HttpMessageHandler> HttpHandlerMock = new(MockBehavior.Loose);

    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    /// <summary>
    /// This constructor is run before every test
    /// </summary>
    /// <param name="output">Sets up the logging system for logging during testing.</param>
    /// <param name="logEventLevel"></param>
    protected BaseUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
    {
        Output = output;
        LogEventLevel = logEventLevel;

        EnvironmentExtensions.EnableUnmaskedLog(true);

        LogManager.SetupLogging(logEventLevel);

        BogusExtensions.Setup();

        var testLogConfig = new TestLogConfig(output);
        Log = testLogConfig.CreateLogInstance<BaseUnitTest>(LogEventLevel);

        Mock = AutoMock.GetStrict(SetDefaultBuilder);
    }

    /// <summary>
    /// Useful for updating private, protected or init properties on an object.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <param name="newValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="InvalidOperationException"></exception>
    protected static void UpdateInitProperty<T>(T obj, string propertyName, object newValue)
    {
        var property = obj
            ?.GetType()
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property == null || !property.CanWrite)
        {
            throw new InvalidOperationException($"Property '{propertyName}' not found or cannot be written to.");
        }

        property.SetValue(obj, newValue);
    }

    protected T SetupEndpointUnitTest<T>()
        where T : class, IEndpoint
    {
        return Factory.Create<T>(ctx =>
        {
            ctx.AddTestServices(s =>
            {
                // All different dependencies that are needed for the endpoint need to be added here. And then they can be mocked in the test.
                s.AddTransient(_ => Mock.Create<ILogger>());
                s.AddTransient(_ => Mock.Create<IReaparrDbContext>());
                s.AddTransient(_ => Mock.Create<IAuthDbContext>());
                s.AddTransient(_ => Mock.Create<ICommandExecutor>());
                s.AddSingleton(_ => Mock.Create<ISchedulerService>());
                s.AddSingleton(_ => Mock.Mock<ISignalRService>().Object);
            });
        });
    }

    public virtual void Dispose()
    {
        if (IsDatabaseSetup)
        {
            MockDatabase.GetMemoryReaparrDbContext(_databaseName).EnsureDeleted();
        }
    }
}

public class BaseUnitTest<TUnitTestClass> : BaseUnitTest
    where TUnitTestClass : class
{
    protected TUnitTestClass Sut => Mock.Create<TUnitTestClass>();

    protected BaseUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(output, logEventLevel) { }

    public override void Dispose()
    {
        base.Dispose();
        Mock.Dispose();
    }
}
