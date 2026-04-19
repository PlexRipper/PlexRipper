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
    /// Serilog level name.
    /// </summary>
    public required LogSeverity Level { get; init; }

    /// <summary>
    /// Rendered log message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Optional exception string.
    /// </summary>
    public required string? Exception { get; init; }

    /// <summary>
    /// Optional source context.
    /// </summary>
    public required string? SourceContext { get; init; }
}
