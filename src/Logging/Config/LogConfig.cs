using Environment;
using Logging.Enricher;
using Logging.Interface;
using Logging.LogGeneric;
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
        Log.Logger = GetLogger(minimumLogLevel);
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

    /// <summary>
    /// Returns a reference to the singleton <see cref="ILog"/> object.
    /// </summary>
    /// <returns></returns>
    public static ILog<T> GetLog<T>(LogEventLevel logLevel = LogEventLevel.Debug) where T : class
    {
        return new LogGeneric<T>(GetLogger(logLevel));
    }

    public static ILog GetLog(Type classType, LogEventLevel logLevel = LogEventLevel.Debug)
    {
        return new LogGeneric<Type>(GetLogger(logLevel), classType);
    }

    #endregion

    #endregion

    private static ITestOutputHelper _testOutput;

    private static ILog _log;

    public static void CloseAndFlush()
    {
        Log.CloseAndFlush();
    }
}