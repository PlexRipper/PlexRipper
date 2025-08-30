using FastEndpoints;
using Reaparr.Data.Contracts;
using ILog = Reaparr.Logging.ILog;

namespace Reaparr.Application;

public record DownloadTaskWorkerLogNotification(IList<DownloadWorkerLog> logs) : IEvent;

public class DownloadTaskWorkerLogNotificationHandler : IEventHandler<DownloadTaskWorkerLogNotification>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public DownloadTaskWorkerLogNotificationHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
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
            _log.Error(e);
        }
    }
}
