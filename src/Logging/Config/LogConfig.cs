using Environment;
using Logging.Enricher;
using Logging.Interface;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Xunit.Abstractions;

namespace Logging;

public static class LogConfig
{
    #region Properties

    private static readonly string _template =
        $"{{NewLine}}{{Timestamp:HH:mm:ss}} [{{Level}}] [{{{ClassNamePropertyName}}}.{{{MemberNamePropertyName}}}:{{{LineNumberPropertyName}}}] => {{Message}}{{NewLine}}{{Exception}}";

    public static MessageTemplateTextFormatter TemplateTextFormatter => new(_template);

    public static string ClassNamePropertyName => "ClassName";
    public static string MemberNamePropertyName => "MemberName";
    public static string LineNumberPropertyName => "LineNumber";

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
            .WriteTo.Debug(outputTemplate: _template)
            .WriteTo.Console(theme: LogTheme.ColoredDark, outputTemplate: _template);
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
                    _template,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 7)
                .MinimumLevel.Is(minimumLogLevel)
                .CreateLogger();
        }

        // Test Logger
        return GetBaseConfiguration()
            .MinimumLevel.Is(minimumLogLevel)
            .WriteTo.TestOutput(_testOutput, minimumLogLevel, _template)
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
            // This works when each file only has 1 class and is named the same
            new(ClassNamePropertyName, new ScalarValue(fileName)),
            new(MemberNamePropertyName, new ScalarValue(memberName)),
            new(LineNumberPropertyName, new ScalarValue(sourceLineNumber)),
        });

        return new LogEvent(dateTimeOffset, logLevel, exception, parsedTemplate, properties);
    }

    #endregion

    #endregion

    private static ITestOutputHelper _testOutput;

    private static ILog _log;

    public static void CloseAndFlush()
    {
        Serilog.Log.CloseAndFlush();
    }
}