using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using Quartz;
using Settings.Contracts;

namespace PlexRipper.Application;

public class DownloadJob : IJob, IDisposable
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly INotificationsService _notificationsService;
    private readonly IDownloadManagerSettingsModule _downloadManagerSettings;
    private readonly IPlexDownloadClient _plexDownloadClient;

    public DownloadJob(
        ILog log,
        IPlexRipperDbContext dbContext,
        INotificationsService notificationsService,
        IDownloadManagerSettingsModule downloadManagerSettings,
        IPlexDownloadClient plexDownloadClient)
    {
        _log = log;
        _dbContext = dbContext;
        _notificationsService = notificationsService;
        _downloadManagerSettings = downloadManagerSettings;
        _plexDownloadClient = plexDownloadClient;
    }

    public static string DownloadTaskIdParameter => "DownloadTaskId";

    public static string PlexServerIdParameter => "PlexServerId";

    public static JobKey GetJobKey(Guid id) => new($"{DownloadTaskIdParameter}_{id}", nameof(DownloadJob));

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var downloadTaskId = dataMap.GetGuidValue(DownloadTaskIdParameter);
        var plexServerId = dataMap.GetIntValue(PlexServerIdParameter);

        var token = context.CancellationToken;
        _log.Debug("Executing job: {DownloadJobName} for {DownloadTaskIdName} with id: {DownloadTaskId}", nameof(DownloadJob), nameof(downloadTaskId),
            downloadTaskId);

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            // Create the multiple download worker tasks which will split up the work
            var downloadTask = await _dbContext.GetDownloadTaskAsync(downloadTaskId, cancellationToken: token);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskId).LogError();
                return;
            }

            if (!downloadTask.IsDownloadable)
            {
                _log.Warning("DownloadTask {DownloadTaskId} is not downloadable, aborting DownloadJob", downloadTaskId);
                return;
            }

            // Create the multiple download worker tasks which will split up the work
            if (!downloadTask.DownloadWorkerTasks.Any())
            {
                var parts = _downloadManagerSettings.DownloadSegments;
                downloadTask.DownloadWorkerTasks = downloadTask.GenerateDownloadWorkerTasks(parts);
                await _dbContext.DownloadWorkerTasks.AddRangeAsync(downloadTask.DownloadWorkerTasks, token);
                await _dbContext.SaveChangesAsync(token);
                _log.Debug("Generated DownloadWorkerTasks for {DownloadTaskFullTitle}", downloadTask.FullTitle);
            }

            SetDownloadAndDestination(downloadTask)

            _log.Debug("Creating Download client for {DownloadTaskFullTitle}", downloadTask.FullTitle);
            var downloadClientResult = await _plexDownloadClient.Setup(downloadTask.ToKey(), token);
            if (downloadClientResult.IsFailed)
            {
                downloadClientResult.LogError();
                return;
            }

            SetupSubscription(_plexDownloadClient);

            var startResult = _plexDownloadClient.Start();
            if (startResult.IsFailed)
                await _notificationsService.SendResult(startResult);

            try
            {
                await _plexDownloadClient.DownloadProcessTask.WaitAsync(token);
            }
            catch (TaskCanceledException)
            {
                _log.Information("{DownloadJobName} with {DownloadTaskIdName}: {DownloadTaskId} has been requested to be stopped", nameof(DownloadJob),
                    nameof(downloadTaskId), downloadTaskId);

                await _plexDownloadClient.StopAsync();
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.Error(ex);
        }
        finally
        {
            _log.Debug("Exiting job: {DownloadJobName} for {DownloadTaskName} with id: {DownloadTaskId}", nameof(DownloadJob), nameof(DownloadTaskGeneric),
                downloadTaskId);
        }
    }

    public void Dispose()
    {
        _log.Here().Warning("Disposing job: {DownloadJobName} for {DownloadTaskName}", nameof(DownloadJob), nameof(DownloadTaskGeneric));
    }

    private void SetupSubscription(IPlexDownloadClient plexDownloadClient)
    {
        plexDownloadClient.ListenToDownloadWorkerLog
            .Select(logs => Observable.Defer(() => CreateLog(logs).ToObservable()))
            .Concat()
            .Subscribe();
    }

    private async Task CreateLog(IList<DownloadWorkerLog> logs)
    {
        if (logs is null || !logs.Any())
            return;

        try
        {
            await _dbContext.DownloadWorkerTasksLogs.AddRangeAsync(logs);
            await _dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.Error(ex);
        }
    }

    private async Task<Result> SetDownloadAndDestination(DownloadTaskFileBase downloadTask)
    {
        if (downloadTask.DownloadDirectory == string.Empty)
        {
            var downloadFolderPath = GetDownloadFolderPath(downloadTask);

            // Set download and destination folder of each downloadable file

            downloadTask.DownloadDirectory = downloadFolderPath;
        }

        if (downloadTask.DestinationDirectory == string.Empty)
        {
            var destinationFolderPath = await _dbContext.GetDestinationFolder(downloadTask.PlexLibraryId);
            if (destinationFolderPath is null)
                return Result.Fail($"Could not find destination folder for PlexLibrary with id {downloadTask.PlexLibraryId}");

            switch (downloadTask.DownloadTaskType)
            {
                case DownloadTaskType.MovieData:
                    downloadTask.DestinationDirectory = Path.Combine(destinationFolderPath.DirectoryPath, downloadTask.FullTitle, downloadTask.FileName);
                    break;
                case DownloadTaskType.EpisodeData:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}