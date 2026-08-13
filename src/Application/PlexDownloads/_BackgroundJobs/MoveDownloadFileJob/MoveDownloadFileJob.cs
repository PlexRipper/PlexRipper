using TickerQ.Utilities.Base;

namespace Reaparr.Application;

public class MoveDownloadFileJob : BaseBackgroundJob<DownloadTaskKey, MoveDownloadFileJobUpdateDTO>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IReaparrDbContext _dbContext;
    private readonly IDownloadTaskUpdateDispatcher _downloadTaskUpdateDispatcher;
    private readonly IMoveDownloadFileQueue _moveDownloadFileQueue;

    public MoveDownloadFileJob(
        ILogger log,
        ICommandExecutor commandExecutor,
        IReaparrDbContext dbContext,
        IDownloadTaskUpdateDispatcher downloadTaskUpdateDispatcher,
        IMoveDownloadFileQueue moveDownloadFileQueue,
        IProgressHubService progressHubService,
        INotificationHubService notificationHubService
    ) : base(log, progressHubService, notificationHubService)
    {
        _log = log.ForContext<MoveDownloadFileJob>();
        _commandExecutor = commandExecutor;
        _dbContext = dbContext;
        _downloadTaskUpdateDispatcher = downloadTaskUpdateDispatcher;
        _moveDownloadFileQueue = moveDownloadFileQueue;
    }

    protected override JobTypes JobType => JobTypes.MoveDownloadFileJob;

    protected override List<RefreshDataType> RefreshDataTypes => [RefreshDataType.DownloadTasks];

    public static JobKey GetJobKey(Guid id) =>
        new($"{nameof(JobTypes.MoveDownloadFileJob)}_{id}", JobTypes.MoveDownloadFileJob);

    protected override async Task ExecuteJobAsync(
        TickerFunctionContext<DownloadTaskKey> context,
        CancellationToken cancellationToken
    )
    {
        var downloadTaskKey = context.Request;

        try
        {
            _log.Here()
                .Information(
                    "Executing job: {NameOfMoveDownloadJob} for {NameOfFileTaskId} with id: {FileTaskId}",
                    nameof(MoveDownloadFileJob),
                    nameof(downloadTaskKey),
                    downloadTaskKey.Id
                );

            var moveResult = await Result.Try(() =>
                _commandExecutor.Send(
                    new MoveDownloadFileFromFileTaskCommand(downloadTaskKey),
                    cancellationToken
                )
            );

            if (moveResult.IsCancelled)
            {
                _log.Here()
                    .Warning(
                        "{NameOfMoveDownloadJob} for {NameOfFileTaskId} with id: {FileTaskId} was cancelled",
                        nameof(MoveDownloadFileJob),
                        nameof(downloadTaskKey),
                        downloadTaskKey.Id
                    );
                return;
            }

            if (moveResult.IsFailed)
            {
                _log.Here().Error("Failed to move all files for {DownloadTaskKey}", downloadTaskKey);
                return;
            }

            var downloadTaskResult = await Result.Try(() =>
                _dbContext.GetDownloadTaskFileAsync(downloadTaskKey, cancellationToken)
            );
            if (downloadTaskResult.IsCancelled)
            {
                _log.Here()
                    .Warning(
                        "{JobName} for {DownloadTaskKey} was cancelled",
                        nameof(MoveDownloadFileJob),
                        downloadTaskKey
                    );
                return;
            }

            if (downloadTaskResult.IsFailed)
            {
                downloadTaskResult.LogError();
                return;
            }

            var downloadTask = downloadTaskResult.Value;
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.Id).LogError();
                return;
            }

            if (downloadTask.DownloadStatus is DownloadStatus.MoveFinished)
            {
                await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                    downloadTaskKey,
                    DownloadStatus.Completed,
                    cancellationToken
                );

                var cleanupResult = await Result.Try(() =>
                    _commandExecutor.Send(
                        new CleanUpDownloadTaskFoldersCommand(downloadTaskKey),
                        cancellationToken
                    )
                );
                if (cleanupResult.IsCancelled)
                {
                    _log.Here()
                        .Warning(
                            "{JobName} for {DownloadTaskKey} was cancelled",
                            nameof(MoveDownloadFileJob),
                            downloadTaskKey
                        );
                    return;
                }

                if (cleanupResult.IsFailed)
                    cleanupResult.LogError();
            }
        }
        catch (Exception e)
        {
            _log.Here()
                .Error(
                    e,
                    "Unexpected error in {JobName} for {DownloadTaskKey}",
                    nameof(MoveDownloadFileJob),
                    downloadTaskKey
                );
        }
        finally
        {
            var queueResult = await _moveDownloadFileQueue.CheckMoveDownloadFileJobQueue(cancellationToken);
            if (queueResult.IsCancelled)
                queueResult.LogWarning();
            else if (queueResult.IsFailed)
                queueResult.LogError();
        }
    }

    protected override Task<MoveDownloadFileJobUpdateDTO?> GetStatusUpdateDataAsync(
        TickerFunctionContext<DownloadTaskKey> context,
        CancellationToken cancellationToken
    ) => Task.FromResult<MoveDownloadFileJobUpdateDTO?>(
        new MoveDownloadFileJobUpdateDTO { DownloadTaskId = context.Request }
    );
}
