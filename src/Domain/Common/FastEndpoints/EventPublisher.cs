using FastEndpoints;
using Reaparr.Logging;
using Reaparr.Logging.Interface;

namespace Reaparr.Domain;

public class EventPublisher : IEventPublisher
{
    private readonly ILog<EventPublisher> _log;

    public EventPublisher(ILog<EventPublisher> log)
    {
        _log = log;
    }

    /// <inheritdoc/>
    public Task PublishAsync(IEvent @event, CancellationToken ct = default) =>
        PublishAsync(@event, Mode.WaitForNone, ct);

    /// <inheritdoc/>
    public async Task PublishAsync(IEvent @event, Mode mode = Mode.WaitForNone, CancellationToken ct = default)
    {
        try
        {
            await @event.PublishAsync(mode, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _log.Warning("Publish of {EventType} canceled.", @event.GetType().Name);
        }
        catch (Exception e)
        {
            // Swallow exception to avoid breaking the fire and forget
            _log.Here().Error("Error publishing event {EventType}: {ErrorMessage}", @event, e.Message);
            _log.Error(e);
        }
    }
}
