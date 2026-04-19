using System.Globalization;

namespace Reaparr.Logging;

public static class LogEventMapper
{
    private static long _sequence;

    public static LiveLogEventDTO ToLiveLogEvent(this LogEvent logEvent) =>
        new()
        {
            Sequence = Interlocked.Increment(ref _sequence),
            Timestamp = logEvent.Timestamp,
            Severity = logEvent.Level.ToLogLevel(),
            Message = logEvent.RenderMessage(CultureInfo.InvariantCulture),
            Exception = logEvent.Exception?.ToString(),
            SourceContext =
                $"{logEvent.GetStringProperty(LogConfig.FileName)}:{logEvent.GetIntProperty(LogConfig.LineNumber)}.{logEvent.GetStringProperty(LogConfig.MethodName)}()",
        };

    public static LogSeverity ToLogLevel(this LogEventLevel source)
    {
        return source switch
        {
            LogEventLevel.Verbose => LogSeverity.Verbose,
            LogEventLevel.Debug => LogSeverity.Debug,
            LogEventLevel.Information => LogSeverity.Information,
            LogEventLevel.Warning => LogSeverity.Warning,
            LogEventLevel.Error => LogSeverity.Error,
            LogEventLevel.Fatal => LogSeverity.Fatal,
            _ => LogSeverity.None,
        };
    }
}
