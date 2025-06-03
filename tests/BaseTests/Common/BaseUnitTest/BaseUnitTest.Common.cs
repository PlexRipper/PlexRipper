using System.Reflection;
using Application.Contracts;
using Data.Contracts;
using Environment;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;

namespace PlexRipper.BaseTests;

public partial class BaseUnitTest
{
    protected readonly ITestOutputHelper _output;
    protected readonly LogEventLevel _logEventLevel;

    protected readonly ILog Log;

    // Use loose behavior here to avoid Dispose() not mocked exception
    protected Mock<HttpMessageHandler> HttpHandlerMock = new(MockBehavior.Loose);

    /// <summary>
    /// This constructor is run before every test
    /// </summary>
    /// <param name="output">Sets up the logging system for logging during testing.</param>
    /// <param name="logEventLevel"></param>
    protected BaseUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
    {
        _output = output;
        _logEventLevel = logEventLevel;

        EnvironmentExtensions.EnableUnmaskedLog(true);

        LogManager.SetupLogging(logEventLevel);
        LogConfig.SetTestOutputHelper(output);
        BogusExtensions.Setup();
        Log = LogManager.CreateLogInstance(output, typeof(BaseUnitTest));

        mock = AutoMock.GetStrict(SetDefaultBuilder);
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
                s.AddTransient(_ => mock.Create<ILog>());
                s.AddTransient(_ => mock.Create<IPlexRipperDbContext>());
                s.AddTransient(_ => mock.Create<ICommandExecutor>());
                s.AddSingleton(_ => mock.Create<IMediator>());
                s.AddSingleton(_ => mock.Create<ISchedulerService>());
                s.AddSingleton(_ => mock.Mock<ISignalRService>().Object);
            });
        });
    }

    public virtual void Dispose()
    {
        if (IsDatabaseSetup)
        {
            MockDatabase.GetMemoryPlexRipperDbContext(_databaseName).EnsureDeleted();
        }
    }
}

public class BaseUnitTest<TUnitTestClass> : BaseUnitTest
    where TUnitTestClass : class
{
    protected TUnitTestClass _sut => mock.Create<TUnitTestClass>();

    protected BaseUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(output, logEventLevel) { }

    public override void Dispose()
    {
        base.Dispose();
        mock.Dispose();
    }
}

public class BaseCommandUnitTest<TCommand, TCommandHandler> : BaseUnitTest
    where TCommandHandler : class, ICommandHandler<TCommand, Result>
    where TCommand : class, ICommand<Result>
{
    protected BaseCommandUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(output, logEventLevel) { }

    private IValidator<TCommand> GetValidator()
    {
        var commandType = typeof(TCommandHandler);
        var validatorTypeName = commandType.FullName!.Replace("Handler", "Validator");

        var validatorType = commandType.Assembly.GetTypes().FirstOrDefault(t => t.FullName == validatorTypeName);

        if (validatorType is null)
            throw new InvalidOperationException($"Validator type '{validatorTypeName}' not found.");

        return (IValidator<TCommand>)Activator.CreateInstance(validatorType)!;
    }

    /// <summary>
    /// Use this method to test the execution of a command handler, including the corresponding validator.
    /// </summary>
    /// <param name="command"> The ICommand to execute inside the handler.</param>
    protected async Task<Result> TestHandlerExecuteAsync(TCommand command)
    {
        var validator = GetValidator();

        var validationResult = await validator.ValidateAsync(command, CancellationToken.None);
        if (!validationResult.IsValid)
            return validationResult.ToResult();

        var handler = mock.Create<TCommandHandler>();
        return await handler.ExecuteAsync(command, CancellationToken.None);
    }

    public override void Dispose()
    {
        base.Dispose();
        mock.Dispose();
    }
}
