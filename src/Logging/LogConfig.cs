using Serilog.Enrichers.Sensitive;
using Serilog.Filters;
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

    private const string TEMPLATE_TEXT =
        "{@t:HH:mm:ss} [{@l}] "
        + "{#if FileName is not null}"
        + "[{FileName}:{LineNumber}.{MethodName}()]"
        + "{#else}"
        + "[{SourceContext}]"
        + "{#end} => {@m}\n{@x}\n";

    // Keep interactive console/debug output colorized, but leave redirected CI/test output plain text.
    protected static readonly ExpressionTemplate ConsoleTemplate = new(
        TEMPLATE_TEXT,
        theme: LogThemes.SystemColored.ToTemplateTheme()
    );

    protected static readonly ExpressionTemplate FileTemplate = new(TEMPLATE_TEXT);

    /// <summary>
    /// Provides a base configuration with console and debug sinks, and allows for extension by derived classes (e.g. to add file or Seq sinks).
    /// </summary>
    /// <param name="minimumLogLevel">The global minimum log level used before sink-specific filtering.</param>
    /// <returns></returns>
    protected static LoggerConfiguration GetBaseConfiguration(LogEventLevel minimumLogLevel = LogEventLevel.Debug)
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLogLevel)
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
                var sensitiveProperties = new[]
                {
                    "PlexLibraryTitle",
                    "PlexAccountDisplayName",
                    "PlexLibraryName",
                    "PlexServerName",
                    "UserName",
                    "PublicAddress",
                    "PlexServerConnectionUrl",
                    "PlexServerConnection",
                    "PlexServerStatus",
                    "DownloadUrl",
                    "AuthToken",
                    "AccessToken",
                    "RefreshToken",
                    "PlexAuthToken",
                    "Token",
                    "Password",
                    "ApiKey",
                    "VerificationCode",
                    "Authorization",
                    "SID",
                    "MachineIdentifier",
                };

                foreach (var property in sensitiveProperties)
                {
                    options.MaskProperties.Add(MaskProperty.WithDefaults(property));
                }
            });
        }

        return config.Enrich.FromLogContext().WriteTo.Debug(ConsoleTemplate).WriteTo.Console(ConsoleTemplate);
    }

    /// <summary>
    /// Provides an extended sink configuration with file and Seq sinks.
    /// </summary>
    /// <param name="minimumLogLevel">The global minimum log level used before sink-specific filtering.</param>
    protected virtual LoggerConfiguration GetExtendedConfiguration(
        LogEventLevel minimumLogLevel = LogEventLevel.Debug
    ) =>
        GetBaseConfiguration(minimumLogLevel)
            .WriteTo.Seq(EnvironmentExtensions.GetSeqUrl(), restrictedToMinimumLevel: minimumLogLevel)
            .WriteTo.File(
                FileTemplate,
                Path.Combine(PathProvider.LogsDirectory, "log.txt"),
                restrictedToMinimumLevel: minimumLogLevel,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 7
            );

    public virtual Logger GetLogger(LogEventLevel minimumLogLevel = LogEventLevel.Debug) =>
        GetExtendedConfiguration(minimumLogLevel).CreateLogger();
}
