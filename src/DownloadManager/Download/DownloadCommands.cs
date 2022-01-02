using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager
{
    public class DownloadCommands : IDownloadCommands
    {
        #region Fields

        private readonly IDownloadQueue _downloadQueue;

        private readonly IDownloadTracker _downloadTracker;

        private readonly IFileSystem _fileSystem;

        private readonly IDirectorySystem _directorySystem;

        private readonly IMediator _mediator;

        private readonly INotificationsService _notificationsService;

        private readonly IDownloadScheduler _downloadScheduler;

        private readonly IDownloadTaskFactory _downloadTaskFactory;

        private readonly IDownloadProgressScheduler _downloadProgressScheduler;

        #endregion

        #region Constructor

        public DownloadCommands(
            IMediator mediator,
            IDownloadTracker downloadTracker,
            IDownloadQueue downloadQueue,
            IFileSystem fileSystem,
            IDirectorySystem directorySystem,
            INotificationsService notificationsService,
            IDownloadScheduler downloadScheduler,
            IDownloadTaskFactory downloadTaskFactory,
            IDownloadProgressScheduler downloadProgressScheduler)
        {
            _mediator = mediator;
            _downloadTracker = downloadTracker;
            _downloadQueue = downloadQueue;
            _fileSystem = fileSystem;
            _directorySystem = directorySystem;
            _notificationsService = notificationsService;
            _downloadScheduler = downloadScheduler;
            _downloadTaskFactory = downloadTaskFactory;
            _downloadProgressScheduler = downloadProgressScheduler;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public async Task<Result> RestartDownloadTasksAsync(List<int> downloadTaskIds)
        {
            if (downloadTaskIds is null || !downloadTaskIds.Any())
            {
                return Result.Fail("Parameter downloadTasks was empty or null").LogError();
            }

            Log.Information($"Restarting {downloadTaskIds.Count} downloadTasks");

            var stopDownloadTasksResult = await StopDownloadTasksAsync(downloadTaskIds);
            if (stopDownloadTasksResult.IsFailed)
            {
                return stopDownloadTasksResult.ToResult().LogError();
            }

            var regeneratedDownloadTasks = await _downloadTaskFactory.RegenerateDownloadTask(stopDownloadTasksResult.Value);
            if (regeneratedDownloadTasks.IsFailed)
            {
                return regeneratedDownloadTasks.LogError();
            }

            await _mediator.Send(new UpdateDownloadTasksByIdCommand(regeneratedDownloadTasks.Value));

            var uniquePlexServers = regeneratedDownloadTasks.Value.Select(x => x.PlexServerId).Distinct().ToList();
            await _downloadQueue.CheckDownloadQueue(uniquePlexServers);

            return Result.Ok();
        }

        public async Task<Result> ResumeDownloadTasksAsync(List<int> downloadTaskIds)
        {
            if (!downloadTaskIds.Any())
                return ResultExtensions.IsEmpty(nameof(downloadTaskIds)).LogWarning();

            var downloadTasksIdsResult = await _mediator.Send(new GetAllRelatedDownloadTaskIdsQuery(downloadTaskIds));
            if (downloadTasksIdsResult.IsFailed)
            {
                return downloadTasksIdsResult;
            }

            foreach (var downloadTaskId in downloadTasksIdsResult.Value)
            {
                // TODO Add Resume download task
                // var downloadClient = _downloadTracker.GetDownloadClient(downloadTaskId);
                // if (downloadClient.IsFailed)
                // {
                //     //downloadClient.Value;
                // }
            }

            return Result.Ok();
        }

        #region Start

        public async Task<Result> StartDownloadTaskAsync(DownloadTask downloadTask)
        {
            if (downloadTask is null)
                return ResultExtensions.IsNull(nameof(downloadTask)).LogWarning();

            if (downloadTask.IsDownloadable)
                return await _downloadScheduler.StartDownloadJob(downloadTask.Id);

            return Result.Fail($"Failed to start downloadTask {downloadTask.FullTitle}, it's not directly downloadable.");
        }

        #endregion

        #region Stop

        /// <inheritdoc/>
        public async Task<Result<List<int>>> StopDownloadTasksAsync(List<int> downloadTaskIds)
        {
            if (!downloadTaskIds.Any())
                return ResultExtensions.IsEmpty(nameof(downloadTaskIds)).LogWarning();

            var allRelatedIds = await _mediator.Send(new GetAllRelatedDownloadTaskIdsQuery(downloadTaskIds));
            if (allRelatedIds.IsFailed)
                return allRelatedIds;

            Log.Information($"Stopping {allRelatedIds.Value.Count} from downloading");

            var stoppedDownloadTaskIds = new List<int>();
            foreach (int downloadTaskId in allRelatedIds.Value)
            {
                var downloadTask = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId));
                if (downloadTask.IsFailed)
                {
                    await _notificationsService.SendResult(downloadTask);
                    continue;
                }

                if (!downloadTask.Value.IsDownloadable)
                {
                    // Check if currently downloading
                    var stopResult = await _downloadTracker.StopDownloadClient(downloadTaskId);
                    if (stopResult.IsFailed)
                    {
                        await _notificationsService.SendResult(stopResult);
                    }

                    var removeTempResult = _directorySystem.DeleteAllFilesFromDirectory(downloadTask.Value.DownloadDirectory);
                    if (removeTempResult.IsFailed)
                    {
                        await _notificationsService.SendResult(removeTempResult);
                    }

                }

                stoppedDownloadTaskIds.Add(downloadTask.Value.Id);
            }

            var updateResult = await _mediator.Send(new UpdateDownloadStatusOfDownloadTaskCommand(stoppedDownloadTaskIds, DownloadStatus.Stopped));
            if (updateResult.IsFailed)
            {
                return updateResult;
            }

            return Result.Ok(stoppedDownloadTaskIds);
        }

        #endregion

        #region Pause

        /// <inheritdoc/>
        public async Task<Result> PauseDownload(List<int> downloadTaskIds)
        {
            if (downloadTaskIds == null)
                return ResultExtensions.IsNull(nameof(downloadTaskIds));

            if (!downloadTaskIds.Any())
                return ResultExtensions.IsEmpty(nameof(downloadTaskIds));

            var failedTasks = new List<int>();
            var errors = new List<Error>();
            foreach (var downloadTaskId in downloadTaskIds)
            {
                // The paused downloading state will be send through an subscription to the database.
                var pauseResult = await _downloadTracker.PauseDownloadClient(downloadTaskId);
                if (pauseResult.IsFailed)
                {
                    failedTasks.Add(downloadTaskId);
                    errors.AddRange(pauseResult.Errors);
                }
            }

            return errors.Any()
                ? Result.Ok()
                : Result.Fail($"Failed to Pause {failedTasks.Count} DownloadTasks, Id's: {failedTasks}").WithErrors(errors);
        }

        #endregion

        /// <inheritdoc/>
        public async Task<Result> ClearCompletedAsync(List<int> downloadTaskIds)
        {
            return await _mediator.Send(new ClearCompletedDownloadTasksCommand(downloadTaskIds));
        }

        /// <inheritdoc/>
        public async Task<Result> DeleteDownloadTaskClientsAsync(List<int> downloadTaskIds)
        {
            if (downloadTaskIds is null || !downloadTaskIds.Any())
            {
                return Result.Fail("Parameter downloadTaskIds was empty or null").LogError();
            }

            var stopResult = await StopDownloadTasksAsync(downloadTaskIds);
            if (stopResult.IsFailed)
            {
                return stopResult.ToResult();
            }

            return await _mediator.Send(new DeleteDownloadTasksByIdCommand(stopResult.Value));
        }

        #endregion
    }
}