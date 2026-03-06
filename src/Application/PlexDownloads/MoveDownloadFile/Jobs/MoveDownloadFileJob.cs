using Microsoft.EntityFrameworkCore;
using Quartz;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

[DisallowConcurrentExecution]
public class MoveDownloadFileJob : IJob
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IReaparrDbContext _dbContext;
    private readonly IMoveDownloadFileQueue _moveDownloadFileQueue;

    public MoveDownloadFileJob(
        ILogger log,
        ICommandExecutor commandExecutor,
        IReaparrDbContext dbContext,
        IMoveDownloadFileQueue moveDownloadFileQueue
    )
    {
        _log = log.ForContext<MoveDownloadFileJob>();
        _commandExecutor = commandExecutor;
        _dbContext = dbContext;
        _moveDownloadFileQueue = moveDownloadFileQueue;
    }

    public const string DownloadTaskIdParameter = "DownloadTaskId";

    public static JobKey GetJobKey(Guid id) => new($"{DownloadTaskIdParameter}_{id}", nameof(MoveDownloadFileJob));

    public async Task Execute(IJobExecutionContext context)
    {
        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        var dataMap = context.JobDetail.JobDataMap;
        var ct = context.CancellationToken;
        var downloadTaskKey = dataMap.GetJsonValue<DownloadTaskKey>(DownloadTaskIdParameter);
        if (downloadTaskKey is null)
        {
            ResultExtensions.IsNull(nameof(DownloadTaskKey)).LogError();
            return;
        }

        async Task QueueNextAsync()
        {
            var queueResult = await Result.Try(() => _moveDownloadFileQueue.CheckMoveDownloadFileJobQueue());
            if (queueResult.IsFailed)
            {
                queueResult.LogError();
            }
        }

        _log.Here()
            .Information(
                "Executing job: {NameOfMoveDownloadJob} for {NameOfFileTaskId} with id: {FileTaskId}",
                nameof(MoveDownloadFileJob),
                nameof(downloadTaskKey),
                downloadTaskKey.Id
            );

        var moveResult = await Result.Try(() =>
            _commandExecutor.Send(new MoveDownloadFileFromFileTaskCommand(downloadTaskKey), ct)
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
            await QueueNextAsync();
            return;
        }

        if (moveResult.IsFailed)
        {
            _log.Here().Error("Failed to move all files for {DownloadTaskKey}", downloadTaskKey);
            await QueueNextAsync();
            return;
        }

        var downloadTaskResult = await Result.Try(() => _dbContext.GetDownloadTaskFileAsync(downloadTaskKey, ct));
        if (downloadTaskResult.IsCancelled)
        {
            _log.Here()
                .Warning("{JobName} for {DownloadTaskKey} was cancelled", nameof(MoveDownloadFileJob), downloadTaskKey);
            await QueueNextAsync();
            return;
        }

        if (downloadTaskResult.IsFailed)
        {
            downloadTaskResult.LogError();
            await QueueNextAsync();
            return;
        }

        var downloadTask = downloadTaskResult.Value;
        if (downloadTask is null)
        {
            ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.Id).LogError();
            await QueueNextAsync();
            return;
        }

        if (downloadTask.DownloadStatus is DownloadStatus.MoveFinished)
        {
            var updateStatusResult = await Result.Try(() =>
                _dbContext.SetDownloadStatus(downloadTaskKey, DownloadStatus.Completed)
            );
            if (updateStatusResult.IsCancelled)
            {
                _log.Here()
                    .Warning(
                        "{JobName} for {DownloadTaskKey} was cancelled",
                        nameof(MoveDownloadFileJob),
                        downloadTaskKey
                    );
                await QueueNextAsync();
                return;
            }

            if (updateStatusResult.IsFailed)
            {
                updateStatusResult.LogError();
                await QueueNextAsync();
                return;
            }

            // Clean up the Download task folders
            var cleanupResult = await Result.Try(() =>
                _commandExecutor.Send(new CleanUpDownloadTaskFoldersCommand(downloadTaskKey), ct)
            );
            if (cleanupResult.IsCancelled)
            {
                _log.Here()
                    .Warning(
                        "{JobName} for {DownloadTaskKey} was cancelled",
                        nameof(MoveDownloadFileJob),
                        downloadTaskKey
                    );
                await QueueNextAsync();
                return;
            }

            if (cleanupResult.IsFailed)
            {
                cleanupResult.LogError();
                await QueueNextAsync();
                return;
            }

            var updatedResult = await Result.Try(() =>
                _commandExecutor.Send(new DownloadTaskUpdatedCommand(downloadTaskKey), ct)
            );
            if (updatedResult.IsCancelled)
            {
                _log.Here()
                    .Warning(
                        "{JobName} for {DownloadTaskKey} was cancelled",
                        nameof(MoveDownloadFileJob),
                        downloadTaskKey
                    );
                await QueueNextAsync();
                return;
            }

            if (updatedResult.IsFailed)
            {
                updatedResult.LogError();
                await QueueNextAsync();
                return;
            }
        }

        await QueueNextAsync();
    }
}
