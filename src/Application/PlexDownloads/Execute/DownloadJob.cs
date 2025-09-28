using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public class DownloadJob : IJob, IDisposable
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IEventPublisher _eventPublisher;
    private readonly IDownloadManagerSettings _downloadManagerSettings;
    private readonly IPlexDownloadClient _plexDownloadClient;

    public DownloadJob(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IEventPublisher eventPublisher,
        IDownloadManagerSettings downloadManagerSettings,
        IPlexDownloadClient plexDownloadClient
    )
    {
        _log = log.ForContext<DownloadJob>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _eventPublisher = eventPublisher;
        _downloadManagerSettings = downloadManagerSettings;
        _plexDownloadClient = plexDownloadClient;
    }

    public static string DownloadTaskIdParameter => "DownloadTaskId";

    public static JobKey GetJobKey(Guid id) => new($"{DownloadTaskIdParameter}_{id}", nameof(DownloadJob));

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var downloadTaskKey = dataMap.GetJsonValue<DownloadTaskKey>(DownloadTaskIdParameter);

        var token = context.CancellationToken;
        _log.Here()
            .Debug(
                "Executing job: {DownloadJobName} for {DownloadTaskIdName} with id: {DownloadTaskId}",
                nameof(DownloadJob),
                nameof(downloadTaskKey),
                downloadTaskKey
            );

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            if (downloadTaskKey is null)
            {
                ResultExtensions.IsNull(nameof(DownloadTaskKey)).LogError();
                return;
            }

            // Create the multiple download worker tasks which will split up the work
            var downloadTask = await _dbContext.GetDownloadTaskFileAsync(downloadTaskKey, token);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTaskFileBase), downloadTaskKey.Id).LogError();
                return;
            }

            if (!downloadTask.IsDownloadable)
            {
                _log.Here()
                    .Warning(
                        "DownloadTask {DownloadTaskId} is not downloadable, aborting DownloadJob",
                        downloadTaskKey
                    );
                return;
            }

            var result = await SetDownloadAndDestination(downloadTask);
            if (result.IsFailed)
            {
                result.LogError();
                return;
            }

            // Create the multiple download worker tasks which will split up the work
            if (!downloadTask.DownloadWorkerTasks.Any())
            {
                var parts = _downloadManagerSettings.DownloadSegments;
                downloadTask.DownloadWorkerTasks = downloadTask.GenerateDownloadWorkerTasks(parts);
                await _dbContext.DownloadWorkerTasks.AddRangeAsync(downloadTask.DownloadWorkerTasks, token);
                await _dbContext.SaveChangesAsync(token);
                _log.Here().Debug("Generated DownloadWorkerTasks for {DownloadTaskFullTitle}", downloadTask.FullTitle);
            }

            downloadTask = result.Value;

            _log.Here().Debug("Creating Download client for {DownloadTaskFullTitle}", downloadTask.FullTitle);
            var downloadClientResult = await _plexDownloadClient.Setup(downloadTask.ToKey(), token);
            if (downloadClientResult.IsFailed)
            {
                downloadClientResult.LogError();
                return;
            }

            SetupSubscription(_plexDownloadClient);

            var startResult = _plexDownloadClient.Start();
            if (startResult.IsFailed)
            {
                await _eventPublisher.PublishAsync(new SendNotificationResult(startResult), token);
                return;
            }

            try
            {
                await _plexDownloadClient.DownloadProcessTask.WaitAsync(token);
            }
            catch (TaskCanceledException)
            {
                _log.Here()
                    .Information(
                        "{DownloadJobName} with {DownloadTaskIdName}: {DownloadTaskId} has been requested to be stopped",
                        nameof(DownloadJob),
                        nameof(downloadTaskKey),
                        downloadTaskKey
                    );
                await _plexDownloadClient.StopAsync();

                await _dbContext.SetDownloadStatus(downloadTaskKey, DownloadStatus.Paused);
                await _commandExecutor.Send(new DownloadTaskUpdatedCommand(downloadTaskKey), token);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.Here().ErrorResult(ex);
        }
        finally
        {
            _log.Here()
                .Debug(
                    "Exiting job: {DownloadJobName} for {DownloadTaskName} with id: {DownloadTaskId}",
                    nameof(DownloadJob),
                    nameof(DownloadTaskGeneric),
                    downloadTaskKey
                );
        }
    }

    public void Dispose()
    {
        _log.Here()
            .Warning(
                "Disposing job: {DownloadJobName} for {DownloadTaskName}",
                nameof(DownloadJob),
                nameof(DownloadTaskGeneric)
            );
    }

    private void SetupSubscription(IPlexDownloadClient plexDownloadClient)
    {
        plexDownloadClient
            .ListenToDownloadWorkerLog.Select(logs => Observable.Defer(() => CreateLog(logs).ToObservable()))
            .Concat()
            .Subscribe();
    }

    private async Task CreateLog(IList<DownloadWorkerLog> logs)
    {
        if (!logs.Any())
            return;

        try
        {
            await _dbContext.DownloadWorkerTasksLogs.AddRangeAsync(logs);
            await _dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.Here().ErrorResult(ex);
        }
    }

    private async Task<Result<DownloadTaskFileBase>> SetDownloadAndDestination(DownloadTaskFileBase downloadTask)
    {
        var downloadFolder = await _dbContext.GetDownloadFolder();
        downloadTask.DirectoryMeta.DownloadRootPath = downloadFolder.DirectoryPath;

        // A custom destination folder can have been set during creation
        if (string.IsNullOrEmpty(downloadTask.DirectoryMeta.DestinationRootPath))
        {
            FolderPath? destinationFolder = null;
            if (downloadTask.DestinationFolderPathId is not null && downloadTask.DestinationFolderPathId > 0)
            {
                destinationFolder = await _dbContext.FolderPaths.GetAsync((int)downloadTask.DestinationFolderPathId);
            }

            destinationFolder ??= await _dbContext.GetDestinationFolder(downloadTask.PlexLibraryId);

            if (destinationFolder is null)
                return ResultExtensions.EntityNotFound(nameof(PlexLibrary), downloadTask.PlexLibraryId).LogError();

            downloadTask.DirectoryMeta.DestinationRootPath = destinationFolder.DirectoryPath;
        }

        switch (downloadTask.DownloadTaskType)
        {
            case DownloadTaskType.MovieData:
                await _dbContext
                    .DownloadTaskMovieFile.Where(x => x.Id == downloadTask.Id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DirectoryMeta, downloadTask.DirectoryMeta));
                break;
            case DownloadTaskType.EpisodeData:
                await _dbContext
                    .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == downloadTask.Id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DirectoryMeta, downloadTask.DirectoryMeta));
                break;
            default:
                return Result.Fail($"DownloadTaskType {downloadTask.DownloadTaskType} is not supported");
        }

        return Result.Ok(downloadTask);
    }
}
