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

    // TemplateTheme.Code uses ANSI escape codes unconditionally, unlike SystemConsoleTheme
    // which uses Console.ForegroundColor and produces no color when stdout is redirected (Rider Run mode).
    // applyThemeWhenOutputIsRedirected: true forces ANSI codes even when Rider's test runner
    // redirects stdout (which normally causes ExpressionTemplate to suppress the theme).
    protected static readonly ExpressionTemplate Template = new(
        // Template
        "{@t:HH:mm:ss} [{@l}] "
            + "{#if FileName is not null}"
            + "[{FileName}:{LineNumber}.{MethodName}()]"
            + "{#else}"
            + "[{SourceContext}]"
            + "{#end} => {@m}\n{@x}\n",
        theme: LogThemes.SystemColored.ToTemplateTheme(),
        applyThemeWhenOutputIsRedirected: true
    );

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
                    var firstChar = property[0];
                    options.MaskProperties.Add(char.ToUpperInvariant(firstChar) + property[1..]);
                    options.MaskProperties.Add(char.ToLowerInvariant(firstChar) + property[1..]);
                }
            });
        }

        return config.Enrich.FromLogContext().WriteTo.Debug(Template).WriteTo.Console(Template);
    }

    public virtual Logger GetLogger(LogEventLevel minimumLogLevel = LogEventLevel.Debug) =>
        GetBaseConfiguration()
            .WriteTo.Seq(EnvironmentExtensions.GetSeqUrl())
            .WriteTo.File(
                Template,
                Path.Combine(PathProvider.LogsDirectory, "log.txt"),
                minimumLogLevel,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 7
            )
            .MinimumLevel.Is(minimumLogLevel)
            .CreateLogger();
}
