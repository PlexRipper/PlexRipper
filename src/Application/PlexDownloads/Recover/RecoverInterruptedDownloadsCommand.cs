namespace Reaparr.Application;

/// <summary>
/// Reset download tasks left in Downloading from a previous run before the scheduler starts.
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

    public async Task<Result> ExecuteAsync(RecoverInterruptedDownloadsCommand command, CancellationToken cancellationToken)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        var movieFileZombies = await dbContext.DownloadTaskMovieFile
            .AsNoTracking()
            .Where(x => x.DownloadStatus == DownloadStatus.Downloading)
            .Select(x => new { x.Id, x.FullTitle, x.PlexServerId, x.PlexLibraryId })
            .ToListAsync(cancellationToken);

        var episodeFileZombies = await dbContext.DownloadTaskTvShowEpisodeFile
            .AsNoTracking()
            .Where(x => x.DownloadStatus == DownloadStatus.Downloading)
            .Select(x => new { x.Id, x.FullTitle, x.PlexServerId, x.PlexLibraryId })
            .ToListAsync(cancellationToken);

        var totalReset = 0;

        foreach (var zombie in movieFileZombies)
        {
            _log.Here()
                .Warning(
                    "Recovering interrupted download task {DownloadTaskId} ({FullTitle}) on PlexServer {PlexServerId} — was left in {DownloadStatus} across a restart, resetting to {ResetStatus}",
                    zombie.Id,
                    zombie.FullTitle,
                    zombie.PlexServerId,
                    DownloadStatus.Downloading,
                    DownloadStatus.AutoPaused
                );

            await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                new DownloadTaskKey
                {
                    Id = zombie.Id,
                    Type = DownloadTaskType.MovieData,
                    PlexServerId = zombie.PlexServerId,
                    PlexLibraryId = zombie.PlexLibraryId,
                },
                DownloadStatus.AutoPaused,
                cancellationToken
            );

            totalReset++;
        }

        foreach (var zombie in episodeFileZombies)
        {
            _log.Here()
                .Warning(
                    "Recovering interrupted download task {DownloadTaskId} ({FullTitle}) on PlexServer {PlexServerId} — was left in {DownloadStatus} across a restart, resetting to {ResetStatus}",
                    zombie.Id,
                    zombie.FullTitle,
                    zombie.PlexServerId,
                    DownloadStatus.Downloading,
                    DownloadStatus.AutoPaused
                );

            await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                new DownloadTaskKey
                {
                    Id = zombie.Id,
                    Type = DownloadTaskType.EpisodeData,
                    PlexServerId = zombie.PlexServerId,
                    PlexLibraryId = zombie.PlexLibraryId,
                },
                DownloadStatus.AutoPaused,
                cancellationToken
            );

            totalReset++;
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
}
