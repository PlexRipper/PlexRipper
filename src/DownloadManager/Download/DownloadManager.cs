using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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
    /// <summary>
    /// Handles all <see cref="DownloadTask"/> management, all download related commands should be handled here.
    /// </summary>
    public class DownloadManager : IDownloadManager
    {
        #region Fields

        private readonly IFileMerger _fileMerger;

        private readonly IMediator _mediator;

        private readonly ISignalRService _signalRService;

        private readonly INotificationsService _notificationsService;

        private readonly IDownloadTracker _downloadTracker;

        private readonly IDownloadTaskValidator _downloadTaskValidator;

        private readonly IDownloadQueue _downloadQueue;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadManager"/> class.
        /// </summary>
        /// <param name="mediator">Defines a mediator to encapsulate request/response and publishing interaction patterns.</param>
        /// <param name="signalRService"></param>
        /// <param name="fileMerger">.</param>
        /// <param name="downloadQueue">Used to retrieve the next <see cref="DownloadTask"/> from the <see cref="DownloadQueue"/>.</param>
        /// <param name="notificationsService"></param>
        /// <param name="downloadTracker"></param>
        /// <param name="downloadTaskValidator"></param>
        public DownloadManager(
            IMediator mediator,
            ISignalRService signalRService,
            IFileMerger fileMerger,
            IDownloadQueue downloadQueue,
            INotificationsService notificationsService,
            IDownloadTracker downloadTracker,
            IDownloadTaskValidator downloadTaskValidator
        )
        {
            _mediator = mediator;
            _signalRService = signalRService;
            _fileMerger = fileMerger;
            _notificationsService = notificationsService;
            _downloadTracker = downloadTracker;
            _downloadTaskValidator = downloadTaskValidator;
            _downloadQueue = downloadQueue;

            SetupSubscriptions();
        }

        #endregion

        #region Methods

        #region Private

        private void SetupSubscriptions()
        {
            // Setup DownloadQueue subscriptions
            _downloadTracker
                .DownloadTaskUpdate
                .SubscribeAsync(UpdateDownloadTaskAsync);

            _downloadTracker
                .DownloadTaskCompleted
                .SubscribeAsync(OnDownloadFileCompleted);

            _downloadQueue
                .UpdateDownloadTasks
                .SubscribeAsync(UpdateDownloadTasksAsync);

            _fileMerger
                .FileMergeProgressObservable
                .SubscribeAsync(OnFileMergeProgress);
        }

        #region Subscriptions

        private async Task OnDownloadFileCompleted(DownloadTask downloadTask)
        {
            if (downloadTask.MediaParts == 1)
            {
                downloadTask.DownloadStatus = DownloadStatus.Moving;
                downloadTask.DownloadWorkerTasks.ForEach(x => x.DownloadStatus = DownloadStatus.Moving);
            }

            if (downloadTask.MediaParts > 1)
            {
                downloadTask.DownloadStatus = DownloadStatus.Merging;
                downloadTask.DownloadWorkerTasks.ForEach(x => x.DownloadStatus = DownloadStatus.Merging);
            }

            await UpdateDownloadTaskAsync(downloadTask);
            await _fileMerger.AddFileTaskFromDownloadTask(downloadTask.Id);

            Log.Information($"The download of {downloadTask.Title} has completed!");
            await _downloadQueue.CheckDownloadQueue();
        }

        private async Task OnFileMergeProgress(FileMergeProgress progress)
        {
            Log.Debug(
                $"Merge Progress: {progress.DataTransferred} / {progress.DataTotal} - {progress.Percentage} - {progress.TransferSpeedFormatted}");
            _signalRService.SendFileMergeProgressUpdate(progress);
            if (progress.Percentage >= 100)
            {
                var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(progress.DownloadTaskId));
                if (downloadTaskResult.IsFailed)
                {
                    downloadTaskResult.LogError();
                    return;
                }

                downloadTaskResult.Value.DownloadStatus = DownloadStatus.Completed;
                await UpdateDownloadTaskAsync(downloadTaskResult.Value);
            }
        }

        private async Task UpdateDownloadTaskAsync(DownloadTask downloadTask)
        {
            Log.Debug(downloadTask.ToString());
            var updateResult = await _mediator.Send(new UpdateDownloadTasksByIdCommand(new List<DownloadTask> { downloadTask }));
            if (updateResult.IsFailed)
            {
                updateResult.LogError();
            }

            _signalRService.SendDownloadTaskUpdate(downloadTask);
        }

        private async Task UpdateDownloadTasksAsync(List<DownloadTask> downloadTasks)
        {
            var updateResult = await _mediator.Send(new UpdateDownloadTasksByIdCommand(downloadTasks));
            if (updateResult.IsFailed)
            {
                updateResult.LogError();
            }

            // TODO Determine if update should be send in a list
            //_signalRService.SendDownloadTaskUpdate(downloadTask);
        }

        #endregion

        #endregion

        #region Public

        #region Commands

        /// <inheritdoc/>
        public async Task<Result> AddToDownloadQueueAsync(List<DownloadTask> downloadTasks)
        {
            if (!downloadTasks.Any())
                return ResultExtensions.IsEmpty(nameof(downloadTasks)).LogWarning();

            Log.Debug($"Attempt to add {downloadTasks.Count} DownloadTasks");
            var validateResult = _downloadTaskValidator.ValidateDownloadTasks(downloadTasks);
            if (validateResult.IsFailed)
            {
                return validateResult.ToResult().LogDebug();
            }

            // Add to Database
            var createResult = await _mediator.Send(new CreateDownloadTasksCommand(validateResult.Value));
            if (createResult.IsFailed)
            {
                return createResult.ToResult().LogError();
            }

            if (createResult.Value.Count != validateResult.Value.Count)
            {
                return Result.Fail("The added download tasks are not stored correctly, missing download tasks.").LogError();
            }

            Log.Debug($"Successfully added all {validateResult.Value.Count} DownloadTasks");
            await _downloadQueue.CheckDownloadQueue();
            return Result.Ok();
        }

        #endregion

        #endregion

        #endregion
    }
}