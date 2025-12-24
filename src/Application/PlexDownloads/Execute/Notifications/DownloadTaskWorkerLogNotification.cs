using FastEndpoints;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record DownloadTaskWorkerLogNotification(IList<DownloadWorkerLog> Logs) : IEvent;

public class DownloadTaskWorkerLogNotificationHandler : IEventHandler<DownloadTaskWorkerLogNotification>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;

    public DownloadTaskWorkerLogNotificationHandler(ILogger log, IReaparrDbContextFactory dbContextFactory)
    {
        _log = log.ForContext<DownloadTaskWorkerLogNotificationHandler>();
        _dbContextFactory = dbContextFactory;
    }

    public async Task HandleAsync(
        DownloadTaskWorkerLogNotification logNotification,
        CancellationToken cancellationToken
    )
    {
        var logs = logNotification.Logs;
        if (!logs.Any())
            return;

        try
        {
            // Create a new DbContext for this operation to avoid threading issues
            using var dbContext = await _dbContextFactory.CreateAsync();
            await dbContext.DownloadWorkerTasksLogs.AddRangeAsync(logs, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _log.Here().ErrorResult(e);
        }
    }
}
