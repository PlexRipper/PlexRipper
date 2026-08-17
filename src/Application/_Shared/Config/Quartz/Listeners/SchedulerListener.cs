using Quartz.Listener;

namespace Reaparr.Application;

public sealed class SchedulerListener(ILogger log) : SchedulerListenerSupport
{
    private readonly ILogger _log = log.ForContext<SchedulerListener>();

    public override Task SchedulerError(
        string message,
        SchedulerException cause,
        CancellationToken cancellationToken = default
    )
    {
        _log.Here().Error(cause, "Quartz scheduler error: {Message}", message);
        return Task.CompletedTask;
    }
}
