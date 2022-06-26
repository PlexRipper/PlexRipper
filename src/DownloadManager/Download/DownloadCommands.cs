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
    /// <summary>
    /// Used to convert batch commands to smaller single commands for the <see cref="DownloadTracker"/>
    /// </summary>
    public class DownloadCommands : IDownloadCommands
    {
        #region Fields

        private readonly IDownloadQueue _downloadQueue;

        private readonly IDownloadTracker _downloadTracker;

        private readonly IDirectorySystem _directorySystem;

        private readonly IMediator _mediator;

        private readonly INotificationsService _notificationsService;

        private readonly IDownloadTaskFactory _downloadTaskFactory;

        #endregion

        #region Constructor

        public DownloadCommands(
            IMediator mediator,
            IDownloadTracker downloadTracker,
            IDownloadQueue downloadQueue,
            IDirectorySystem directorySystem,
            INotificationsService notificationsService,
            IDownloadTaskFactory downloadTaskFactory)
        {
            _mediator = mediator;
            _downloadTracker = downloadTracker;
            _downloadQueue = downloadQueue;
            _directorySystem = directorySystem;
            _notificationsService = notificationsService;
            _downloadTaskFactory = downloadTaskFactory;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public async Task<Result> RestartDownloadTasksAsync(int downloadTaskId)
        {
            if (downloadTaskId <= 0)
                return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            var stopDownloadTasksResult = await StopDownloadTasksAsync(downloadTaskId);
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

        public async Task<Result> ResumeDownloadTasksAsync(int downloadTaskId)
        {
            if (downloadTaskId <= 0)
                return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            var downloadTasksResult = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId, true));
            if (downloadTasksResult.IsFailed)
                return downloadTasksResult.ToResult();

            if (_downloadTracker.IsDownloading(downloadTaskId))
            {
                Log.Information(
                    $"PlexServer {downloadTasksResult.Value.PlexServer.Name} already has a DownloadTask downloading so another one cannot be started");
                return Result.Ok();
            }

            var nextTask = _downloadQueue.GetNextDownloadTask(new List<DownloadTask> { downloadTasksResult.Value });
            if (nextTask.IsFailed)
                return nextTask.ToResult();

            return await StartDownloadTaskAsync(nextTask.Value);
        }

        #region Start

        public async Task<Result> StartDownloadTaskAsync(DownloadTask downloadTask)
        {
            if (downloadTask is null)
                return ResultExtensions.IsNull(nameof(downloadTask)).LogWarning();

            if (downloadTask.IsDownloadable)
                return await _downloadTracker.StartDownloadClient(downloadTask.Id);

            return Result.Fail($"Failed to start downloadTask {downloadTask.FullTitle}, it's not directly downloadable.");
        }

        #endregion

        #region Stop

        /// <inheritdoc/>
        public async Task<Result<List<int>>> StopDownloadTasksAsync(int downloadTaskId)
        {
            if (downloadTaskId <= 0)
                return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            var stoppedDownloadTaskIds = new List<int>();

            var downloadTask = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId));
            if (downloadTask.IsFailed)
            {
                await _notificationsService.SendResult(downloadTask);
                return downloadTask.ToResult();
            }

            Log.Information($"Stopping {downloadTask.Value.FullTitle} from downloading");

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

            stoppedDownloadTaskIds.Add(downloadTask.Value.Id);

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
        public async Task<Result> PauseDownload(int downloadTaskId)
        {
            if (downloadTaskId <= 0)
                return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            var failedTasks = new List<int>();
            var errors = new List<IError>();

            // The paused downloading state will be send through an subscription to the database.
            var pauseResult = await _downloadTracker.PauseDownloadClient(downloadTaskId);
            if (pauseResult.IsFailed)
            {
                failedTasks.Add(downloadTaskId);
                errors.AddRange(pauseResult.Errors);
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
        public async Task<Result<bool>> DeleteDownloadTaskClientsAsync(List<int> downloadTaskIds)
        {
            if (downloadTaskIds is null || !downloadTaskIds.Any())
            {
                return Result.Fail("Parameter downloadTaskIds was empty or null").LogError();
            }

            foreach (var downloadTaskId in downloadTaskIds)
            {
                if (_downloadTracker.IsDownloading(downloadTaskId))
                {
                    await StopDownloadTasksAsync(downloadTaskId);
                }
            }

            return await _mediator.Send(new DeleteDownloadTasksByIdCommand(downloadTaskIds));
        }

        #endregion
    }
}