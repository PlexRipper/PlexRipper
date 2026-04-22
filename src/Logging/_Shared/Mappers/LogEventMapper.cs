using System.Globalization;

namespace Reaparr.Logging;

public static class LogEventMapper
{
    private static long _sequence;

    public static LiveLogEventDTO ToLiveLogEvent(this LogEvent logEvent)
    {
        var fileName = logEvent.GetStringProperty(LogConfig.FileName);
        var lineNumber = logEvent.GetIntProperty(LogConfig.LineNumber);
        var methodName = logEvent.GetStringProperty(LogConfig.MethodName);
        var hasSourceContext =
            !string.IsNullOrWhiteSpace(fileName) || lineNumber != 0 || !string.IsNullOrWhiteSpace(methodName);

        return new LiveLogEventDTO
        {
            Sequence = Interlocked.Increment(ref _sequence),
            Timestamp = logEvent.Timestamp,
            Level = logEvent.Level.ToLogLevel(),
            Message = logEvent.RenderMessage(CultureInfo.InvariantCulture),
            Exception = logEvent.Exception?.ToString(),
            SourceContext = hasSourceContext ? $"{fileName}:{lineNumber}.{methodName}()" : null,
        };
    }

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
