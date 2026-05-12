using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Reaparr.Application;

/// <summary>
/// The DownloadQueue is responsible for deciding which downloadTask is handled.
/// </summary>
public class DownloadQueue : IDownloadQueue
{
    /// <summary>
    /// Cooldown applied to every download task after it is picked by the queue. Prevents the
    /// queue picker from re-picking the same task in a tight loop when a download fails fast
    /// (e.g. stale Plex part IDs returning 404 instantly).
    /// </summary>
    private static readonly TimeSpan RetryCooldown = TimeSpan.FromSeconds(60);

    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    private readonly Channel<int> _plexServersToCheckChannel = Channel.CreateUnbounded<int>();
    private readonly ConcurrentDictionary<Guid, DateTime> _retryCooldownUntil = new();

    private readonly CancellationToken _token = new();

    public DownloadQueue(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        IDownloadTaskScheduler downloadTaskScheduler
    )
    {
        _log = log.ForContext<DownloadQueue>();
        _dbContextFactory = dbContextFactory;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    public bool IsBusy => _plexServersToCheckChannel.Reader.Count > 0;

    public Result Setup()
    {
        var copyTask = Task.Factory.StartNew(ExecuteDownloadQueueCheck, TaskCreationOptions.LongRunning);
        return copyTask.IsFaulted ? Result.Fail("ExecuteFileTasks failed due to an error").LogError() : Result.Ok();
    }

    /// <summary>
    /// Check the DownloadQueue for downloadTasks which can be started.
    /// </summary>
    public async Task<Result> CheckDownloadQueue(List<int> plexServerIds)
    {
        if (!plexServerIds.Any())
            return ResultExtensions.IsEmpty(nameof(plexServerIds)).LogWarning();

        _log.Here()
            .Information(
                "Adding {PlexServerIdsCount} {NameOfPlexServer}s to the DownloadQueue to check for the next download",
                plexServerIds.Count,
                nameof(PlexServer)
            );
        foreach (var plexServerId in plexServerIds)
            await _plexServersToCheckChannel.Writer.WriteAsync(plexServerId, _token);

        return Result.Ok();
    }

    internal async Task<Result<DownloadTaskGeneric>> CheckDownloadQueueServer(int plexServerId)
    {
        if (plexServerId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId).LogWarning();

        // Create a new DbContext for this operation to avoid threading issues
        using var dbContext = await _dbContextFactory.CreateAsync();

        var plexServerName = await dbContext.GetPlexServerNameById(plexServerId);

        if (await dbContext.IsDownloadsPausedByUser(plexServerId))
        {
            _log.Here()
                .Information(
                    "Skipping download queue check because PlexServer {PlexServerName} is paused by user.",
                    plexServerName
                );
            return Result.Ok();
        }

        if (await dbContext.IsServerDisabled(plexServerId))
        {
            _log.Here()
                .Information(
                    "Skipping download queue check because PlexServer {PlexServerName} is disabled.",
                    plexServerName
                );
            return Result.Ok();
        }

        // Check if the server is online
        if (!await dbContext.IsServerOnline(plexServerId, cancellationToken: _token))
        {
            return _log.Here()
                .WarningResult(
                    "PlexServer with name: {PlexServerName} is not online, cannot continue checking the DownloadQueue to pick the following download",
                    plexServerName
                );
        }

        var downloadTasks = await dbContext.GetAllDownloadTasksByServerAsync(plexServerId, cancellationToken: _token);

        var hasDownloadingTask = downloadTasks.Any(x => x.DownloadStatus == DownloadStatus.Downloading);

        // This avoids race condition where job is finishing but still registered in Quartz
        if (hasDownloadingTask && await _downloadTaskScheduler.IsServerDownloading(plexServerId))
        {
                return Result
                    .Fail("Cannot select the next download task because server is already downloading one.")
                    .LogWarning();
            }

        _log.Here()
            .Debug(
                "Checking {NameOfPlexServer}: {PlexServerName} for the next download to start",
                nameof(PlexServer),
                plexServerName
            );
        var nextDownloadTaskResult = GetNextDownloadTask(downloadTasks);
        if (nextDownloadTaskResult.IsFailed)
        {
            _log.Here()
                .Information(
                    "There are no available downloadTasks remaining for PlexServer with Id: {PlexServerName}",
                    plexServerName
                );
            return Result.Ok();
        }

        var nextDownloadTask = nextDownloadTaskResult.Value;

        _log.Here()
            .Information(
                "Selected download task {NextDownloadTaskFullTitle} to start as the next task",
                nextDownloadTask.FullTitle
            );

        _retryCooldownUntil[nextDownloadTask.Id] = DateTime.UtcNow + RetryCooldown;
        await _downloadTaskScheduler.StartDownloadTaskJob(nextDownloadTask.ToKey());

        return Result.Ok(nextDownloadTask);
    }

    private bool IsInRetryCooldown(DownloadTaskGeneric task) =>
        _retryCooldownUntil.TryGetValue(task.Id, out var until) && DateTime.UtcNow < until;

    /// <summary>
    /// Determines the next downloadable <see cref="DownloadTaskGeneric"/> to be executed.
    /// </summary>
    /// <param name="downloadTasks"> The list of downloadTasks to check for the next downloadable task.</param>
    /// <returns> The next downloadable <see cref="DownloadTaskGeneric"/> to be executed.</returns>
    internal Result<DownloadTaskGeneric> GetNextDownloadTask(ICollection<DownloadTaskGeneric> downloadTasks)
    {
        var downloadingTask = FindFirstLeafByStatus(downloadTasks, DownloadStatus.Downloading);
        if (downloadingTask is not null)
            return Result.Fail("There is already a downloadTask downloading.").LogDebug();

        var autoPausedTask = FindFirstLeafByStatus(downloadTasks, DownloadStatus.AutoPaused);
        if (autoPausedTask is not null)
            return Result.Ok(autoPausedTask);
        
        var serverUnreachableTask = FindFirstLeafByStatus(downloadTasks, DownloadStatus.ServerUnreachable);
        if (serverUnreachableTask is not null)
            return Result.Ok(serverUnreachableTask);

        var downloadClientErrorTask = FindFirstLeafByStatus(downloadTasks, DownloadStatus.DownloadClientError);
        if (downloadClientErrorTask is not null)
            return Result.Ok(downloadClientErrorTask);

        var errorTask = FindFirstLeafByStatus(downloadTasks, DownloadStatus.Error);
        if (errorTask is not null)
            return Result.Ok(errorTask);

        var queuedTask = FindFirstLeafByStatus(downloadTasks, DownloadStatus.Queued);
        if (queuedTask is not null)
            return Result.Ok(queuedTask);

        return Result.Fail("There were no downloadTasks left to download.").LogDebug();
    }

    private static DownloadTaskGeneric? FindFirstLeafByStatus(
        IEnumerable<DownloadTaskGeneric> downloadTasks,
        DownloadStatus status
    )
    {
        foreach (var downloadTask in downloadTasks)
        {
            if (downloadTask.Children.Any())
            {
                var childTask = FindFirstLeafByStatus(downloadTask.Children, status);
                if (childTask is not null)
                    return childTask;

                continue;
            }

            if (downloadTask.DownloadStatus == status)
                return downloadTask;
        }

        return null;
    }

    private async Task ExecuteDownloadQueueCheck()
    {
        while (!_token.IsCancellationRequested)
        {
            var item = await _plexServersToCheckChannel.Reader.ReadAsync(_token);
            await CheckDownloadQueueServer(item);
        }
    }
}
