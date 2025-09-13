using FastEndpoints;

namespace Reaparr.Domain;

/// <summary>
/// Defines a contract for publishing domain events with configurable execution modes.
/// This interface provides methods to publish events either in fire-and-forget mode
/// or with specific execution behavior as defined by the Mode parameter.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes the specified domain event in fire-and-forget mode.
    /// The method returns immediately without waiting for event handlers to complete execution.
    /// This is useful for non-critical events where immediate response is required.
    /// Note: Exceptions in event handlers are logged but do not propagate to the caller.
    /// </summary>
    /// <param name="event">The domain event to publish</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous publish operation</returns>
    Task PublishAsync(IEvent @event, CancellationToken ct = default);

    /// <summary>
    /// Publishes the specified domain event with the given execution mode.
    /// The Mode parameter controls how event handlers are processed and when the method returns.
    /// The default mode is WaitForNone that provides fire-and-forget behavior.
    /// </summary>
    /// <param name="event">The domain event to publish</param>
    /// <param name="mode">The execution mode that determines how event handlers are processed</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous publish operation</returns>
    Task PublishAsync(IEvent @event, Mode mode = Mode.WaitForNone, CancellationToken ct = default);
}
