using Reaparr.Environment;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers.Sensitive;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Display;
using Serilog.Sinks.Console.LogThemes;
using Serilog.Templates;

namespace Reaparr.Logging;

public class LogConfig
{
    public static string FileName => nameof(FileName);
    public static string FilePath => nameof(FilePath);
    public static string MethodName => nameof(MethodName);

    public static string LineNumber => nameof(LineNumber);

    public static string SourceContext => nameof(SourceContext);

    private static readonly string _template =
        $"{{NewLine}}{{Timestamp:HH:mm:ss}} [{{Level}}] [{{{FileName}}}.cs:{{{LineNumber}}}.{{{MethodName}}}()] => {{Message:lj}}{{NewLine}}{{Exception}}";

    private static readonly ExpressionTemplate _newTemplate = new(
        // Template
        "{@t:HH:mm:ss} [{@l:u3}] "
            + "{#if FileName is not null}"
            + "[{FileName}:{LineNumber}.{MethodName}()]"
            + "{#else}"
            + "[{SourceContext}]"
            + "{#end} => {@m}\n{@x}",
        theme: LogThemes.SystemColored.ToTemplateTheme()
    );

    protected static readonly MessageTemplateTextFormatter TemplateTextFormatter = new(_template);

    protected static LoggerConfiguration GetBaseConfiguration()
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            // These filters: No XML encryptor configured. Key {*} may be persisted to storage in unencrypted form.
            // This can be ignored because we use proper auth: https://github.com/dotnet/aspnetcore/issues/3309#issuecomment-404246838
            .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager"))
            .MinimumLevel.Override("Quartz", LogEventLevel.Warning);

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

        return config.Enrich.FromLogContext().WriteTo.Debug(_newTemplate).WriteTo.Console(_newTemplate);
    }

    public virtual Logger GetLogger(LogEventLevel minimumLogLevel = LogEventLevel.Debug) =>
        GetBaseConfiguration()
            .WriteTo.Seq("http://localhost:5341")
            .WriteTo.File(
                new ConditionalTextFormatter(),
                Path.Combine(PathProvider.LogsDirectory, "log.txt"),
                minimumLogLevel,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 7
            )
            .MinimumLevel.Is(minimumLogLevel)
            .CreateLogger();

    public ILogger CreateLogInstance<T>()
        where T : class => GetLogger().ForContext<T>();

    public ILogger CreateLogInstance<T>(LogEventLevel minimumLogLevel)
        where T : class => GetLogger(minimumLogLevel).ForContext<T>();

    /// <summary>
    /// Returns a new typed <see cref="ILog"/> instance.
    /// </summary>
    /// <returns></returns>
    public ILogger CreateLogInstance(Type classType) => GetLogger().ForContext(classType);
}
