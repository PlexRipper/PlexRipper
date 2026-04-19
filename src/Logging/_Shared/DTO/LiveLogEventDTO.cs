namespace Reaparr.Logging;

/// <summary>
/// Represents a live log event streamed to the frontend.
/// </summary>
public class LiveLogEventDTO
{
    /// <summary>
    /// Monotonic sequence number assigned on the server.
    /// </summary>
    public long Sequence { get; set; }

    /// <summary>
    /// UTC timestamp for when the event was written.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Serilog level name.
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Rendered log message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional exception string.
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// Optional source context.
    /// </summary>
    public string? SourceContext { get; set; }
}
