using System.Runtime.CompilerServices;

namespace Reaparr.Logging;

public static partial class LogExtensions
{
    public static bool IsLogLevelEnabled(this ILogger logger, LogEventLevel logLevel) => logger.IsEnabled(logLevel);

    public static bool IsLogLevelVerbose(this ILogger logger) => logger.IsEnabled(LogEventLevel.Verbose);

    public static bool IsLogLevelDebug(this ILogger logger) => logger.IsEnabled(LogEventLevel.Debug);

    public static ILogger Here(
        this ILogger logger,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) =>
        logger // TODO Rename these, they are too generic and are continuously overridden accidentally
            .ForContext(nameof(SlimLogConfig.FileName), Path.GetFileName(sourceFilePath))
            .ForContext(nameof(SlimLogConfig.FilePath), sourceFilePath)
            .ForContext(nameof(SlimLogConfig.MethodName), memberName)
            .ForContext(nameof(SlimLogConfig.LineNumber), sourceLineNumber);

    public static string RenderMessage(this ILogger log, string messageTemplate, params object[] args)
    {
        log.BindMessageTemplate(messageTemplate, args, out var parsedTemplate, out var boundProperties);

        if (parsedTemplate is null)
        {
            // Fallback: return the original template if parsing fails
            return messageTemplate;
        }

        var logEvent = new LogEvent(
            DateTimeOffset.Now,
            LogEventLevel.Information,
            exception: null,
            messageTemplate: parsedTemplate,
            properties: boundProperties ?? []
        );

        return logEvent.RenderMessage();
    }

    public static string GetStringProperty(this LogEvent logEvent, string propertyName) =>
        logEvent.Properties.TryGetValue(propertyName, out var propertyValue)
            ? propertyValue.ToString().Trim('"')
            : string.Empty;

    public static int? GetIntProperty(this LogEvent logEvent, string propertyName) =>
        int.TryParse(logEvent.GetStringProperty(propertyName), out var value) ? value : null;
}
