using Environment;
using Logging.Enricher;
using Logging.Interface;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Xunit.Abstractions;

namespace Logging;

public class LogConfig
{
    private static ITestOutputHelper _testOutput;
    private static ILog _log;

    protected LogConfig()
    {
        _log = new Log2.Log(GetLogger());
    }

    private static string Template => "{NewLine}{Timestamp:HH:mm:ss} [{Level}] [{FileName}.{MemberName}:{LineNumber}] => {Message}{NewLine}{Exception}";

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
}