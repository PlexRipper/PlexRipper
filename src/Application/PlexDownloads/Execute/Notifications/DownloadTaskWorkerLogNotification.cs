using FastEndpoints;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record DownloadTaskWorkerLogNotification(IList<DownloadWorkerLog> logs) : IEvent;

public class DownloadTaskWorkerLogNotificationHandler : IEventHandler<DownloadTaskWorkerLogNotification>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public DownloadTaskWorkerLogNotificationHandler(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<DownloadTaskWorkerLogNotificationHandler>();
        _dbContext = dbContext;
    }

    public async Task HandleAsync(
        DownloadTaskWorkerLogNotification logNotification,
        CancellationToken cancellationToken
    )
    {
        var logs = logNotification.logs;
        if (!logs.Any())
            return;

        try
        {
            await _dbContext.DownloadWorkerTasksLogs.AddRangeAsync(logs, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _log.Here().ErrorResult(e);
        }
    }
}
