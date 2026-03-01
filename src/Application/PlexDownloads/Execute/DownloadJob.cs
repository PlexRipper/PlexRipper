using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Autofac.Features.Indexed;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public class DownloadJob : IJob
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IEventPublisher _eventPublisher;
    private readonly IDownloadManagerSettings _downloadManagerSettings;
    private readonly IServerSettingsModule _serverSettingsModule;
    private readonly IIndex<PlexDownloadClientType, IPlexDownloadClient> _plexDownloadClientFactory;

    public DownloadJob(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IEventPublisher eventPublisher,
        IDownloadManagerSettings downloadManagerSettings,
        IServerSettingsModule serverSettingsModule,
        IIndex<PlexDownloadClientType, IPlexDownloadClient> plexDownloadClientFactory
    )
    {
        _log = log.ForContext<DownloadJob>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _eventPublisher = eventPublisher;
        _downloadManagerSettings = downloadManagerSettings;
        _serverSettingsModule = serverSettingsModule;
        _plexDownloadClientFactory = plexDownloadClientFactory;
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

            downloadTask = result.Value;

            var machineId = await _dbContext.GetPlexServerMachineIdentifierById(downloadTask.PlexServerId, token);
            var clientType = _serverSettingsModule.GetAllowStreamDownloader(machineId)
                ? PlexDownloadClientType.Dash
                : PlexDownloadClientType.Direct;
            _log.Here()
                .Information(
                    "Creating {ClientType} download client for {DownloadTaskFullTitle}",
                    clientType,
                    downloadTask.FullTitle
                );

            using var plexDownloadClient = _plexDownloadClientFactory[clientType];

            var startResult = await plexDownloadClient.Start(downloadTask.ToKey(), token);

            if (startResult.IsCancelled)
            {
                _log.Here()
                    .Information(
                        "{DownloadJobName} with {DownloadTaskIdName}: {DownloadTaskId} has been requested to be stopped",
                        nameof(DownloadJob),
                        nameof(downloadTaskKey),
                        downloadTaskKey
                    );
                await plexDownloadClient.StopAsync();

                await _dbContext.SetDownloadStatus(downloadTaskKey, DownloadStatus.Paused);
                await _commandExecutor.Send(new DownloadTaskUpdatedCommand(downloadTaskKey), token);
            }

            if (startResult.IsFailed)
            {
                await _eventPublisher.PublishAsync(new SendNotificationResult(startResult), token);
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
