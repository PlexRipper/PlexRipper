using Autofac.Features.Indexed;

using TickerQ.Utilities.Base;

namespace Reaparr.Application;


public class DownloadJob : BaseBackgroundJob<DownloadTaskKey, DownloadJobUpdateDTO>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IReaparrDbContext _dbContext;
    private readonly IDownloadTaskUpdateDispatcher _downloadTaskUpdateDispatcher;
    private readonly IEventPublisher _eventPublisher;
    private readonly IMoveDownloadFileQueue _moveDownloadFileQueue;
    private readonly IIndex<PlexDownloadClientType, IPlexDownloadClient> _plexDownloadClientFactory;

    public DownloadJob(
        ILogger log,
        ICommandExecutor commandExecutor,
        IReaparrDbContext dbContext,
        IDownloadTaskUpdateDispatcher downloadTaskUpdateDispatcher,
        IEventPublisher eventPublisher,
        IMoveDownloadFileQueue moveDownloadFileQueue,
        IIndex<PlexDownloadClientType, IPlexDownloadClient> plexDownloadClientFactory,
        IProgressHubService progressHubService,
        INotificationHubService notificationHubService
    ) : base(log, progressHubService, notificationHubService)
    {
        _log = log.ForContext<DownloadJob>();
        _commandExecutor = commandExecutor;
        _dbContext = dbContext;
        _downloadTaskUpdateDispatcher = downloadTaskUpdateDispatcher;
        _eventPublisher = eventPublisher;
        _moveDownloadFileQueue = moveDownloadFileQueue;
        _plexDownloadClientFactory = plexDownloadClientFactory;
    }

    protected override JobTypes JobType => JobTypes.DownloadJob;

    protected override List<RefreshDataType> RefreshDataTypes => [RefreshDataType.DownloadTasks];

    public static JobKey GetJobKey(Guid id) =>
        new($"{nameof(JobTypes.DownloadJob)}_{id}", JobTypes.DownloadJob);

    protected override async Task ExecuteJobAsync(
        TickerFunctionContext<DownloadTaskKey> context,
        CancellationToken cancellationToken
    )
    {
        var downloadTaskKey = context.Request;
        var token = cancellationToken;

        _log.Here()
            .Debug(
                "Executing job: {DownloadJobName} for {DownloadTaskIdName} with id: {DownloadTaskId}",
                nameof(DownloadJob),
                nameof(downloadTaskKey),
                downloadTaskKey
            );

        try
        {
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

            var result = await SetDownloadAndDestination(downloadTask, token);
            if (result.IsCancelled)
            {
                result.LogWarning();
                return;
            }

            if (result.IsFailed)
            {
                result.LogError();
                return;
            }

            downloadTask = result.Value;

            var clientTypeResult = await _commandExecutor.Send(
                new DeterminePlexDownloadClientCommand(
                    downloadTask.PlexServerId,
                    downloadTask.ToKey(),
                    $"/library/metadata/{downloadTask.PlexApiRatingKey}"
                ),
                token
            );
            if (clientTypeResult.IsCancelled)
            {
                clientTypeResult.LogWarning();
                return;
            }

            if (clientTypeResult.IsFailed)
            {
                await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                    downloadTask.ToKey(),
                    DownloadStatus.DownloadClientError,
                    clientTypeResult.ToResult(),
                    token
                );
                await _eventPublisher.PublishAsync(new SendNotificationResult(clientTypeResult.ToResult()), token);
                return;
            }

            var clientType = clientTypeResult.Value;
            _log.Here()
                .Information(
                    "Creating {ClientType} download client for {DownloadTaskFullTitle}",
                    clientType,
                    downloadTask.FullTitle
                );

            await using var plexDownloadClient = _plexDownloadClientFactory[clientType];

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

                await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                    downloadTaskKey,
                    DownloadStatus.Paused,
                    CancellationToken.None
                );
            }
            else if (startResult.IsFailed)
            {
                var failedStatus =
                    startResult.Has404NotFoundError() ? DownloadStatus.SourceUnavailable
                    : startResult.IsServerUnreachable() ? DownloadStatus.ServerUnreachable
                    : startResult.HasStorageError() ? DownloadStatus.StorageError
                    : DownloadStatus.DownloadClientError;

                await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                    downloadTask.ToKey(),
                    failedStatus,
                    startResult,
                    CancellationToken.None
                );

                await _eventPublisher.PublishAsync(new SendNotificationResult(startResult), token);
            }
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

    protected override Task<DownloadJobUpdateDTO?> GetStatusUpdateDataAsync(
        TickerFunctionContext<DownloadTaskKey> context,
        CancellationToken cancellationToken
    ) => Task.FromResult<DownloadJobUpdateDTO?>(new DownloadJobUpdateDTO { Id = context.Request });

    protected override async Task ExecuteAfterCompletionAsync(
        TickerFunctionContext<DownloadTaskKey> context,
        CancellationToken cancellationToken
    )
    {
        var downloadTaskKey = context.Request;
        var status = await _dbContext.GetDownloadTaskStatusAsync(downloadTaskKey, cancellationToken);

        if (status == DownloadStatus.DownloadFinished)
        {
            _log.Here()
                .Debug(
                    "DownloadTask with id: {DownloadTaskId} has finished downloading, starting moveDownloadJob and executing DownloadQueueCheck",
                    downloadTaskKey.Id
                );
            await _moveDownloadFileQueue.CheckMoveDownloadFileJobQueue(cancellationToken);
        }

        _log.Here()
            .Debug(
                "DownloadTask with id: {DownloadTaskId} ended with status {DownloadStatus}, executing DownloadQueueCheck",
                downloadTaskKey.Id,
                status
            );
        await _eventPublisher.PublishAsync(
            new CheckDownloadQueueEvent(downloadTaskKey.PlexServerId),
            cancellationToken
        );
    }

    private async Task<Result<DownloadTaskFileBase>> SetDownloadAndDestination(
        DownloadTaskFileBase downloadTask,
        CancellationToken cancellationToken
    )
    {
        var downloadFolder = await _dbContext.GetDownloadFolder();
        downloadTask.DirectoryMeta.DownloadRootPath = downloadFolder.DirectoryPath;

        // A custom destination folder can have been set during creation
        if (string.IsNullOrEmpty(downloadTask.DirectoryMeta.DestinationRootPath))
        {
            FolderPath? destinationFolder = null;
            if (downloadTask.DestinationFolderPathId is not null && downloadTask.DestinationFolderPathId > 0)
            {
                destinationFolder = await _dbContext.FolderPaths.GetAsync(
                    (int)downloadTask.DestinationFolderPathId,
                    cancellationToken
                );
            }

            destinationFolder ??= await _dbContext.GetDestinationFolder(
                downloadTask.PlexLibraryId
            );

            if (destinationFolder is null)
                return ResultExtensions.EntityNotFound(nameof(PlexLibrary), downloadTask.PlexLibraryId).LogError();

            downloadTask.DirectoryMeta.DestinationRootPath = destinationFolder.DirectoryPath;
        }

        switch (downloadTask.DownloadTaskType)
        {
            case DownloadTaskType.MovieData:
                await _dbContext
                    .DownloadTaskMovieFile.Where(x => x.Id == downloadTask.Id)
                    .ExecuteUpdateAsync(
                        p => p.SetProperty(x => x.DirectoryMeta, downloadTask.DirectoryMeta),
                        cancellationToken
                    );
                break;
            case DownloadTaskType.EpisodeData:
                await _dbContext
                    .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == downloadTask.Id)
                    .ExecuteUpdateAsync(
                        p => p.SetProperty(x => x.DirectoryMeta, downloadTask.DirectoryMeta),
                        cancellationToken
                    );
                break;
            default:
                return Result.Fail($"DownloadTaskType {downloadTask.DownloadTaskType} is not supported");
        }

        return Result.Ok(downloadTask);
    }
}
