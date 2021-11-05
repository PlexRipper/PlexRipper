using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.Domain.RxNet;

namespace PlexRipper.DownloadManager
{
    public class DownloadCommands : IDownloadCommands
    {
        #region Fields

        private readonly IDownloadQueue _downloadQueue;

        private readonly IDownloadTracker _downloadTracker;

        private readonly IFileSystem _fileSystem;

        private readonly IMediator _mediator;

        private readonly INotificationsService _notificationsService;

        private readonly IDownloadScheduler _downloadScheduler;

        private readonly IPlexDownloadTaskFactory _plexDownloadTaskFactory;

        #endregion

        #region Constructor

        public DownloadCommands(
            IMediator mediator,
            IDownloadTracker downloadTracker,
            IDownloadQueue downloadQueue,
            IFileSystem fileSystem,
            INotificationsService notificationsService,
            IDownloadScheduler downloadScheduler,
            IPlexDownloadTaskFactory plexDownloadTaskFactory)
        {
            _mediator = mediator;
            _downloadTracker = downloadTracker;
            _downloadQueue = downloadQueue;
            _fileSystem = fileSystem;
            _notificationsService = notificationsService;
            _downloadScheduler = downloadScheduler;
            _plexDownloadTaskFactory = plexDownloadTaskFactory;

            SetupSubscriptions();
        }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public async Task<Result> PauseDownload(List<int> downloadTaskIds)
        {
            if (downloadTaskIds == null)
                return ResultExtensions.IsNull(nameof(downloadTaskIds));

            if (!downloadTaskIds.Any())
                return ResultExtensions.IsEmpty(nameof(downloadTaskIds));

            foreach (var downloadTaskId in downloadTaskIds)
            {
                var client = _downloadTracker.GetDownloadClient(downloadTaskId);
                if (client.IsSuccess)
                {
                    // The paused downloading state will be send through an subscription to the database.
                    var downloadTask = await client.Value.PauseAsync();
                    _downloadTracker.DeleteDownloadClient(downloadTaskId);
                }
            }

            return Result.Ok();
        }

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

            var regeneratedDownloadTasks = await _plexDownloadTaskFactory.RegenerateDownloadTask(stopDownloadTasksResult.Value);
            if (regeneratedDownloadTasks.IsFailed)
            {
                return regeneratedDownloadTasks.LogError();
            }

            await _mediator.Send(new UpdateDownloadTasksByIdCommand(regeneratedDownloadTasks.Value));

            await _downloadQueue.CheckDownloadQueue();

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
                var downloadClient = _downloadTracker.GetDownloadClient(downloadTaskId);
                if (downloadClient.IsFailed)
                {
                    //downloadClient.Value;
                }
            }

            return Result.Ok();
        }

        #region Start

        public async Task<Result> StartDownloadTasksAsync(List<int> downloadTaskIds)
        {
            if (downloadTaskIds is null || !downloadTaskIds.Any())
            {
                return Result.Fail("Parameter downloadTasks was empty or null").LogError();
            }

            var downloadTasksResult = await _mediator.Send(new GetAllRelatedDownloadTasksQuery(downloadTaskIds));
            if (downloadTasksResult.IsFailed)
            {
                return downloadTasksResult;
            }

            foreach (var downloadTask in downloadTasksResult.Value)
            {
                await StartDownloadTaskAsync(downloadTask);
            }

            return Result.Ok();
        }

        public async Task<Result> StartDownloadTaskAsync(DownloadTask downloadTask)
        {
            if (downloadTask is null)
                return ResultExtensions.IsNull(nameof(downloadTask)).LogWarning();

            if (downloadTask.IsDownloadable)
                return await _downloadScheduler.StartDownloadJob(downloadTask.Id);

            return Result.Fail($"Failed to start downloadTask {downloadTask.FullTitle}, it's not directly downloadable.");
        }

        #endregion

        /// <inheritdoc/>
        public async Task<Result<List<int>>> StopDownloadTasksAsync(List<int> downloadTaskIds)
        {
            if (downloadTaskIds is null || !downloadTaskIds.Any())
            {
                return ResultExtensions.IsEmpty(nameof(downloadTaskIds)).LogWarning();
            }

            var allRelatedIds = await _mediator.Send(new GetAllRelatedDownloadTaskIdsQuery(downloadTaskIds));
            if (allRelatedIds.IsFailed)
            {
                return allRelatedIds;
            }

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

                if (!downloadTask.Value.IsDownloadTaskPart())
                {
                    // Check if currently downloading
                    var downloadClient = _downloadTracker.GetDownloadClient(downloadTaskId);
                    if (downloadClient.IsSuccess)
                    {
                        var stopResult = await downloadClient.Value.StopAsync();
                        if (stopResult.IsFailed)
                        {
                            await _notificationsService.SendResult(stopResult);
                        }

                        _downloadTracker.DeleteDownloadClient(downloadTask.Value.Id);
                    }

                    var removeTempResult = _fileSystem.DeleteAllFilesFromDirectory(downloadTask.Value.DownloadDirectory);
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

        #region Private Methods

        private void SetupSubscriptions()
        {
            // Setup DownloadQueue subscriptions
            _downloadQueue
                .StartDownloadTask
                .SubscribeAsync(async downloadTask => await StartDownloadTaskAsync(downloadTask));
        }

        #endregion
    }
}