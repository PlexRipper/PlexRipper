namespace Reaparr.Logging;

/// <summary>
/// Represents a live log event streamed to the frontend.
/// </summary>
public class LiveLogEventDTO
{
    /// <summary>
    /// Monotonic sequence number assigned on the server.
    /// </summary>
    public required long Sequence { get; init; }

    /// <summary>
    /// UTC timestamp for when the event was written.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// LogSeverity value representing the log level.
    /// </summary>
    public required LogSeverity Level { get; init; }

    /// <summary>
    /// Rendered log message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Optional exception string.
    /// </summary>
    public string? Exception { get; init; }

    /// <summary>
    /// Optional source context.
    /// </summary>
    public string? SourceContext { get; init; }

    public override string ToString()
    {
        var time = Timestamp.ToString("HH:mm:ss");
        var level = Level.ToString();
        var location = string.IsNullOrWhiteSpace(SourceContext) ? "Unknown" : SourceContext;

        var logLine = $"{time} [{level}] [{location}] => {Message}";

        if (Exception != null)
            logLine += $"{System.Environment.NewLine}{Exception}";

        return logLine;
    }
}
