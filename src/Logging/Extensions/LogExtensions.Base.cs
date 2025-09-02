using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Events;

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
    ) => logger
        .ForContext(nameof(LogConfig.FileName), Path.GetFileNameWithoutExtension(sourceFilePath))
        .ForContext(nameof(LogConfig.MethodName), memberName)
        .ForContext(nameof(LogConfig.LineNumber), sourceLineNumber);

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
}