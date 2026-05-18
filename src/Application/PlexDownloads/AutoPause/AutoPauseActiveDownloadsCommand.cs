namespace Reaparr.Application;

public record AutoPauseActiveDownloadsCommand : ICommand<Result>;

public class AutoPauseActiveDownloadsCommandHandler : ICommandHandler<AutoPauseActiveDownloadsCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    public AutoPauseActiveDownloadsCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IDownloadTaskScheduler downloadTaskScheduler
    )
    {
        _log = log.ForContext<AutoPauseActiveDownloadsCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    public async Task<Result> ExecuteAsync(AutoPauseActiveDownloadsCommand command, CancellationToken cancellationToken)
    {
        var plexServerIds = await _dbContext.PlexServers
            .AsNoTracking()
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var totalPaused = 0;

        foreach (var plexServerId in plexServerIds)
        {
            var activeKeys = await _downloadTaskScheduler.GetCurrentlyDownloadingKeysByServer(plexServerId);

            foreach (var activeKey in activeKeys.Distinct())
            {
                _log.Here()
                    .Information(
                        "Auto-pausing active download task {DownloadTaskKey} during shutdown",
                        activeKey
                    );

                var pauseResult = await _commandExecutor.Send(
                    new PauseDownloadTaskCommand(activeKey.Id, AutoPause: true),
                    cancellationToken
                );

                if (pauseResult.IsFailed)
                    return pauseResult.LogError();

                totalPaused++;
            }
        }

        if (totalPaused > 0)
            _log.Here().Information("Auto-paused {Count} active download task(s) during shutdown", totalPaused);

        return Result.Ok();
    }
}
