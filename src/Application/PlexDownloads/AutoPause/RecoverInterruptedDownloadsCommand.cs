namespace Reaparr.Application;

/// <summary>
///  Reset any download tasks left in Downloading from a previous run before the scheduler starts. Without this, the queue picker treats a zombie task as an active download and never picks a new one for that server.
/// </summary>
public record RecoverInterruptedDownloadsCommand : ICommand<Result>;

public class RecoverInterruptedDownloadsCommandHandler : ICommandHandler<RecoverInterruptedDownloadsCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly IDownloadTaskUpdateDispatcher _downloadTaskUpdateDispatcher;

    public RecoverInterruptedDownloadsCommandHandler(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        IDownloadTaskUpdateDispatcher downloadTaskUpdateDispatcher
    )
    {
        _log = log.ForContext<RecoverInterruptedDownloadsCommandHandler>();
        _dbContextFactory = dbContextFactory;
        _downloadTaskUpdateDispatcher = downloadTaskUpdateDispatcher;
    }

    public async Task<Result> ExecuteAsync(
        RecoverInterruptedDownloadsCommand command,
        CancellationToken cancellationToken)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        var plexServerIds = await dbContext.PlexServers
            .AsNoTracking()
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var totalReset = 0;
        foreach (var plexServerId in plexServerIds)
        {
            var downloadTasks = await dbContext.GetAllDownloadTasksByServerAsync(
                plexServerId,
                cancellationToken: cancellationToken
            );
            var zombies = FindAllLeavesByStatus(downloadTasks, DownloadStatus.Downloading);
            foreach (var zombie in zombies)
            {
                _log.Here()
                    .Warning(
                        "Recovering interrupted download task {DownloadTaskId} ({FullTitle}) on PlexServer {PlexServerId} — was left in {DownloadStatus} across a restart, resetting to {ResetStatus}",
                        zombie.Id,
                        zombie.FullTitle,
                        plexServerId,
                        DownloadStatus.Downloading,
                        DownloadStatus.AutoPaused
                    );
                await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                    zombie.ToKey(),
                    DownloadStatus.AutoPaused,
                    cancellationToken
                );
                totalReset++;
            }
        }

        if (totalReset > 0)
        {
            _log.Here()
                .Information(
                    "Recovered {Count} interrupted download task(s) left in {DownloadStatus} from a previous run",
                    totalReset,
                    DownloadStatus.Downloading
                );
        }

        return Result.Ok();
    }

    private static List<DownloadTaskGeneric> FindAllLeavesByStatus(
        IEnumerable<DownloadTaskGeneric> downloadTasks,
        DownloadStatus status
    )
    {
        var matches = new List<DownloadTaskGeneric>();
        CollectLeavesByStatus(downloadTasks, status, matches);
        return matches;
    }

    private static void CollectLeavesByStatus(
        IEnumerable<DownloadTaskGeneric> downloadTasks,
        DownloadStatus status,
        List<DownloadTaskGeneric> matches
    )
    {
        foreach (var downloadTask in downloadTasks)
        {
            if (downloadTask.Children.Any())
            {
                CollectLeavesByStatus(downloadTask.Children, status, matches);
                continue;
            }

            if (downloadTask.DownloadStatus == status)
                matches.Add(downloadTask);
        }
    }
}