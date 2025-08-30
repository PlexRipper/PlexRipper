using System.Threading.Channels;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Logging;
using ILog = Reaparr.Logging.ILog;

namespace Reaparr.Application;

/// <summary>
/// The DownloadQueue is responsible for deciding which downloadTask is handled.
/// </summary>
public class DownloadQueue : IDownloadQueue
{
    private readonly ILog _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    private readonly Channel<int> _plexServersToCheckChannel = Channel.CreateUnbounded<int>();

    private readonly CancellationToken _token = new();

    public DownloadQueue(ILog log, IReaparrDbContext dbContext, IDownloadTaskScheduler downloadTaskScheduler)
    {
        _log = log;
        _dbContext = dbContext;
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

        var plexServerName = await _dbContext.GetPlexServerNameById(plexServerId, _token);

        // Check if the server is online
        if (!await _dbContext.IsServerOnline(plexServerId, cancellationToken: _token))
        {
            return _log.Warning(
                    "PlexServer with name: {PlexServerName} is not online, cannot continue checking the DownloadQueue to pick the following download",
                    plexServerName
                )
                .ToResult();
        }

        if (await _downloadTaskScheduler.IsServerDownloading(plexServerId))
        {
            return Result
                .Fail("Cannot select the next download task because server is already downloading one.")
                .LogWarning();
        }

        var downloadTasks = await _dbContext.GetAllDownloadTasksByServerAsync(plexServerId, cancellationToken: _token);

        _log.Here()
            .Debug(
                "Checking {NameOfPlexServer}: {PlexServerName} for the next download to start",
                nameof(PlexServer),
                plexServerName
            );
        var nextDownloadTaskResult = GetNextDownloadTask(downloadTasks);
        if (nextDownloadTaskResult.IsFailed)
        {
            _log.Information(
                "There are no available downloadTasks remaining for PlexServer with Id: {PlexServerName}",
                plexServerName
            );
            return Result.Ok();
        }

        var nextDownloadTask = nextDownloadTaskResult.Value;

        _log.Information(
            "Selected download task {NextDownloadTaskFullTitle} to start as the next task",
            nextDownloadTask.FullTitle
        );

        await _downloadTaskScheduler.StartDownloadTaskJob(nextDownloadTask.ToKey());

        return Result.Ok(nextDownloadTask);
    }

    /// <summary>
    /// Determines the next downloadable <see cref="DownloadTaskGeneric"/> to be executed.
    /// </summary>
    /// <param name="downloadTasks"> The list of downloadTasks to check for the next downloadable task.</param>
    /// <returns> The next downloadable <see cref="DownloadTaskGeneric"/> to be executed.</returns>
    internal Result<DownloadTaskGeneric> GetNextDownloadTask(ICollection<DownloadTaskGeneric> downloadTasks)
    {
        List<DownloadStatus> statusCheck =
        [
            DownloadStatus.Downloading,
            DownloadStatus.ServerUnreachable,
            DownloadStatus.Queued,
        ];

        foreach (var status in statusCheck)
        {
            var nextDownloadTask = downloadTasks.FirstOrDefault(x => x.DownloadStatus == status);
            if (nextDownloadTask is not null)
            {
                // Should we check deeper for any nested tasks
                if (nextDownloadTask.Children.Any())
                    return GetNextDownloadTask(nextDownloadTask.Children);

                switch (status)
                {
                    case DownloadStatus.ServerUnreachable:
                    case DownloadStatus.Queued:
                        return Result.Ok(nextDownloadTask);
                    case DownloadStatus.Downloading:
                        return Result.Fail("There is already a downloadTask downloading.").LogDebug();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        return Result.Fail("There were no downloadTasks left to download.").LogDebug();
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
