using Environment;
using Logging.Common;
using Logging.Enricher;
using Logging.Interface;
using Logging.Masks;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers.Sensitive;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Display;
using Serilog.Sinks.Console.LogThemes;

namespace Logging;

public class LogConfig
{
    private static readonly string _template =
        $"{{NewLine}}{{Timestamp:HH:mm:ss}} [{{Level}}] [{{{nameof(LogMetaData.ClassName)}}}.cs:{{{nameof(LogMetaData.LineNumber)}}}.{{{nameof(LogMetaData.MethodName)}}}()] => {{Message:lj}}{{NewLine}}{{Exception}}";

    protected static MessageTemplateTextFormatter TemplateTextFormatter => new(_template);

    protected static LoggerConfiguration GetBaseConfiguration()
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            // These filters: No XML encryptor configured. Key {*} may be persisted to storage in unencrypted form.
            // This can be ignored because we use proper auth: https://github.com/dotnet/aspnetcore/issues/3309#issuecomment-404246838
            .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager"))
            .MinimumLevel.Override("Quartz", LogEventLevel.Warning)
            .Enrich.FromLogContext();

        // Do not mask data when debugging
        if (!EnvironmentExtensions.IsUnmasked())
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
            .WriteTo.Debug(outputTemplate: _template)
            .WriteTo.Console(theme: LogThemes.SystemColored, outputTemplate: _template);
    }

    public virtual Logger GetLogger(LogEventLevel minimumLogLevel = LogEventLevel.Debug) =>
        GetBaseConfiguration()
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

    public ILog CreateLogInstance() => new Log(GetLogger());

    public ILog<T> CreateLogInstance<T>()
        where T : class => new Log<T>(GetLogger(), typeof(T));

    public ILog<T> CreateLogInstance<T>(LogEventLevel minimumLogLevel)
        where T : class => new Log<T>(GetLogger(minimumLogLevel), typeof(T));

    /// <summary>
    /// Returns a new typed <see cref="ILog"/> instance.
    /// </summary>
    /// <returns></returns>
    public ILog CreateLogInstance(Type classType) => new Log<Type>(GetLogger(), classType);
}
