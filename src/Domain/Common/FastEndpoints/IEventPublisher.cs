using FastEndpoints;

namespace PlexRipper.Domain;

public interface IEventPublisher
{
    public Task PublishAsync(IEvent @event, CancellationToken ct = default);

    public Task PublishAsync(IEvent @event, Mode mode = Mode.WaitForNone, CancellationToken ct = default);
}
