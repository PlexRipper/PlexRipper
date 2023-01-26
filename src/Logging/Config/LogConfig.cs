using Environment;
using Logging.Enricher;
using Logging.Interface;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.SystemConsole.Themes;
using Xunit.Abstractions;

namespace Logging;

public class LogConfig
{
    #region Properties

    private static string Template => "{NewLine}{Timestamp:HH:mm:ss} [{Level}] [{FileName}.{MemberName}:{LineNumber}] => {Message}{NewLine}{Exception}";

    public static MessageTemplateTextFormatter TemplateTextFormatter => new(Template);

    #endregion

    #region Methods

    #region Private

    private static LoggerConfiguration GetBaseConfiguration()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Quartz", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.With<ExternalFrameworkEnricher>()
            .WriteTo.Debug(outputTemplate: Template)
            .WriteTo.Console(theme: SystemConsoleTheme.Colored, outputTemplate: Template);
    }

    #endregion

    #region Public

    public static void SetTestOutputHelper(ITestOutputHelper output)
    {
        _testOutput = output;
    }

    public static void SetupLogging(LogEventLevel minimumLogLevel = LogEventLevel.Debug)
    {
        Serilog.Log.Logger = GetLogger(minimumLogLevel);
    }

    public static Logger GetLogger(LogEventLevel minimumLogLevel = LogEventLevel.Debug)
    {
        if (!EnvironmentExtensions.IsIntegrationTestMode())
        {
            return GetBaseConfiguration()
                .WriteTo.File(
                    Path.Combine(PathProvider.LogsDirectory, "log.txt"),
                    LogEventLevel.Debug,
                    Template,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 7)
                .MinimumLevel.Is(minimumLogLevel)
                .CreateLogger();
        }

        // Test Logger
        return GetBaseConfiguration()
            .MinimumLevel.Is(minimumLogLevel)
            .WriteTo.TestOutput(_testOutput, minimumLogLevel, Template)
            .WriteTo.TestCorrelator(minimumLogLevel)
            .CreateLogger();
    }

    /// <summary>
    /// Returns a reference to the singleton <see cref="ILog"/> object.
    /// </summary>
    /// <returns></returns>
    public static ILog GetLog(LogEventLevel logLevel = LogEventLevel.Debug)
    {
        return _log ??= new Log2.Log(GetLogger(logLevel));
    }

    public static LogEvent ToLogEvent(
        LogEventLevel logLevel,
        string messageTemplate,
        Exception exception = default,
        string memberName = default!,
        string sourceFilePath = default!,
        int sourceLineNumber = default!,
        params object?[]? propertyValues)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Serilog.Log.Logger.BindMessageTemplate(messageTemplate, propertyValues, out var parsedTemplate, out var boundProperties);

        var dateTimeOffset = DateTimeOffset.Now;
        var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);

        var properties = boundProperties.ToList();
        properties.AddRange(new List<LogEventProperty>()
        {
            new("FileName", new ScalarValue(fileName)),
            new("MemberName", new ScalarValue(memberName)),
            new("LineNumber", new ScalarValue(sourceLineNumber)),
        });

        return new LogEvent(dateTimeOffset, logLevel, exception, parsedTemplate, properties);
    }

    #endregion

    #endregion

    private static ITestOutputHelper _testOutput;

    private static ILog _log;
}