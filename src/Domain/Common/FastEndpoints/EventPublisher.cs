using FastEndpoints;
using Logging.Interface;

namespace PlexRipper.Domain;

public class EventPublisher : IEventPublisher
{
    private readonly ILog<EventPublisher> _log;

    public EventPublisher(ILog<EventPublisher> log)
    {
        _log = log;
    }

    public Task PublishAsync(IEvent @event, CancellationToken ct = default) =>
        PublishAsync(@event, Mode.WaitForNone, ct);

    public async Task PublishAsync(IEvent @event, Mode mode = Mode.WaitForNone, CancellationToken ct = default)
    {
        try
        {
            await @event.PublishAsync(mode, ct);
        }
        catch (Exception e)
        {
            _log.Error(e);
            throw;
        }
    }
}
