using Serilog.Filters;
using Serilog.Sinks.Console.LogThemes;
using Serilog.Templates;

namespace Reaparr.Logging;

public class SlimLogConfig
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
        + "{#end} => {@m}\n"
        + "{#if Exception is not null}\n{@x}\n{#end}";

    // Keep interactive console/debug output colorized, but leave redirected CI/test output plain text.
    protected static readonly ExpressionTemplate ConsoleTemplate = new(
        TEMPLATE_TEXT,
        theme: LogThemes.SystemColored.ToTemplateTheme()
    );

    protected static readonly ExpressionTemplate FileTemplate = new(TEMPLATE_TEXT);

    protected virtual LoggerConfiguration GetBaseConfiguration(LogEventLevel minimumLogLevel = LogEventLevel.Debug)
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLogLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            // These filters: No XML encryptor configured. Key {*} may be persisted to storage in unencrypted form.
            // This can be ignored because we use proper auth: https://github.com/dotnet/aspnetcore/issues/3309#issuecomment-404246838
            .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager"))
            .MinimumLevel.Override("Quartz", LogEventLevel.Warning);

        return config.Enrich.FromLogContext().WriteTo.Debug(ConsoleTemplate).WriteTo.Console(ConsoleTemplate);
    }

    public virtual Logger GetLogger(LogEventLevel minimumLogLevel = LogEventLevel.Debug) =>
        GetBaseConfiguration(minimumLogLevel).CreateLogger();
}
