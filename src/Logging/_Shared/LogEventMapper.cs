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
            Level = logEvent.Level.ToString(),
            Message = logEvent.RenderMessage(CultureInfo.InvariantCulture),
            Exception = logEvent.Exception?.ToString(),
            SourceContext = TryGetScalarValue(logEvent, LogConfig.SourceContext),
        };

    private static string? TryGetScalarValue(LogEvent logEvent, string propertyName)
    {
        if (!logEvent.Properties.TryGetValue(propertyName, out var value) || value is not ScalarValue scalarValue)
        {
            return null;
        }

        return scalarValue.Value?.ToString();
    }
}
