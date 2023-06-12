using Environment;
using Logging.Common;
using Logging.Enricher;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.Console.LogThemes;
using Xunit.Abstractions;

namespace Logging;

public static class LogConfig
{
    #region Properties

    public static MessageTemplateTextFormatter TemplateTextFormatter => new(Template);

    #endregion

    #region Methods

    #region Public

    public static LoggerConfiguration GetBaseConfiguration()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Quartz", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.With<ExternalFrameworkEnricher>()
            .WriteTo.Debug(outputTemplate: Template)
            .WriteTo.Console(theme: LogThemes.SystemColored, outputTemplate: Template);
    }

    public static void SetTestOutputHelper(ITestOutputHelper output)
    {
        _testOutput = output;
    }

    public static Logger GetLogger(LogEventLevel minimumLogLevel = LogEventLevel.Debug)
    {
        if (_testOutput is null)
        {
            return GetBaseConfiguration()
                .WriteTo.File(
                    TemplateTextFormatter,
                    Path.Combine(PathProvider.LogsDirectory, "log.txt"),
                    LogEventLevel.Debug,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 7)
                .MinimumLevel.Is(minimumLogLevel)
                .CreateLogger();
        }

        // Test Logger
        return GetBaseConfiguration()
            .WriteTo.TestOutput(_testOutput, TemplateTextFormatter, minimumLogLevel)
            .WriteTo.TestCorrelator(minimumLogLevel)
            .MinimumLevel.Is(minimumLogLevel)
            .CreateLogger();
    }

    #endregion

    #endregion

    public static readonly string Template =
        $"{{NewLine}}{{Timestamp:HH:mm:ss}} [{{Level}}] [{{{nameof(LogMetaData.ClassName)}}}.{{{nameof(LogMetaData.MethodName)}}}:{{{nameof(LogMetaData.LineNumber)}}}] => {{Message:lj}}{{NewLine}}{{Exception}}";

    private static ITestOutputHelper _testOutput;
}