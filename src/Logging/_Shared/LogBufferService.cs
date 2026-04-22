using System.Collections.Concurrent;

namespace Reaparr.Logging;

/// <summary>
/// In-memory cache for live log events and functions as a sink.
/// </summary>
public class LogBufferService : ILogBufferService
{
    private readonly ConcurrentQueue<LiveLogEventDTO> _logEvents = new();

    /// <inheritdoc />
    public void Add(LiveLogEventDTO logEvent)
    {
        _logEvents.Enqueue(logEvent);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<LiveLogEventDTO> GetAll() => _logEvents.ToArray();

    public void Emit(LogEvent logEvent)
    {
        Add(logEvent.ToLiveLogEvent());
    }
}
