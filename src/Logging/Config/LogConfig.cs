using System.Diagnostics;
using Environment;
using Logging.Common;
using Logging.Enricher;
using Logging.Masks;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers.Sensitive;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.Console.LogThemes;
using Xunit.Abstractions;

namespace Logging;

public static class LogConfig
{
    public static MessageTemplateTextFormatter TemplateTextFormatter => new(Template);

    public static LoggerConfiguration GetBaseConfiguration()
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Quartz", LogEventLevel.Warning)
            .Enrich.FromLogContext();

        // Do not mask data when debugging
        if (!Debugger.IsAttached)
        {
            config.Enrich.WithSensitiveDataMasking(options =>
            {
                options.MaskingOperators.Clear();
                options.MaskingOperators = [new EmailAddressMaskingOperator(), new UrlMaskingOperator()];
                options.MaskProperties.Add("PlexLibraryTitle");
                options.MaskProperties.Add("PlexAccountDisplayName");
                options.MaskProperties.Add("PlexLibraryName");
                options.MaskProperties.Add("PlexServerName");
                options.MaskProperties.Add("UserName");
                options.MaskProperties.Add("PublicAddress");
                options.MaskProperties.Add("PlexServerConnectionUrl");
                options.MaskProperties.Add("PlexServerConnection");
                options.MaskProperties.Add("PlexServerStatus");
                options.MaskProperties.Add("DownloadUrl");
                options.MaskProperties.Add("AuthToken");
                options.MaskProperties.Add("MachineIdentifier");
            });
        }

        return config
            .Enrich.With<ExternalFrameworkEnricher>()
            .WriteTo.Debug(outputTemplate: Template)
            .WriteTo.Console(theme: LogThemes.SystemColored, outputTemplate: Template);
    }

    public static void SetTestOutputHelper(ITestOutputHelper output)
    {
        _testOutput = output;
    }

    public static Logger GetLogger()
    {
        var minimumLogLevel = LogManager.MinimumLogLevel;
        if (_testOutput is null)
        {
            return GetBaseConfiguration()
                .WriteTo.File(
                    TemplateTextFormatter,
                    Path.Combine(PathProvider.LogsDirectory, "log.txt"),
                    minimumLogLevel,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 7
                )
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

    public static readonly string Template =
        $"{{NewLine}}{{Timestamp:HH:mm:ss}} [{{Level}}] [{{{nameof(LogMetaData.ClassName)}}}.{{{nameof(LogMetaData.MethodName)}}}:{{{nameof(LogMetaData.LineNumber)}}}] => {{Message:lj}}{{NewLine}}{{Exception}}";

    private static ITestOutputHelper _testOutput = null!;
}