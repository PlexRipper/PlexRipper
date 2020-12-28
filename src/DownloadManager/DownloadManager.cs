using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexDownloads;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Common;
using PlexRipper.DownloadManager.Download;

namespace PlexRipper.DownloadManager
{
    /// <summary>
    /// Handles all <see cref="DownloadTask"/> management, all download related commands should be handled here.
    /// </summary>
    public class DownloadManager : DownloadManagerBase, IDownloadManager
    {
        #region Fields

        /// <summary>
        /// Currently loaded and active <see cref="PlexDownloadClient"/>s.
        /// </summary>
        private readonly List<PlexDownloadClient> _downloadsList = new List<PlexDownloadClient>();

        private readonly IFileMerger _fileMerger;

        private readonly IPlexAuthenticationService _plexAuthenticationService;

        private readonly IUserSettings _userSettings;

        private readonly INotificationsService _notificationsService;

        private readonly Func<DownloadTask, PlexDownloadClient> _plexDownloadClientFactory;

        private readonly IDownloadQueue _downloadQueue;

        private Task<Task> _checkDownloadTask;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadManager"/> class.
        /// </summary>
        /// <param name="mediator">Defines a mediator to encapsulate request/response and publishing interaction patterns.</param>
        /// <param name="signalRService"></param>
        /// <param name="plexAuthenticationService">.</param>
        /// <param name="fileSystem">.</param>
        /// <param name="fileMerger">.</param>
        /// <param name="userSettings"></param>
        /// <param name="downloadQueue">Used to retrieve the next <see cref="DownloadTask"/> from the <see cref="DownloadQueue"/>.</param>
        /// <param name="notificationsService"></param>
        /// <param name="plexDownloadClientFactory"></param>
        /// <param name="httpClientFactory"></param>
        public DownloadManager(
            IMediator mediator,
            ISignalRService signalRService,
            IPlexAuthenticationService plexAuthenticationService,
            IFileMerger fileMerger,
            IUserSettings userSettings,
            IDownloadQueue downloadQueue,
            INotificationsService notificationsService,
            Func<DownloadTask, PlexDownloadClient> plexDownloadClientFactory) : base(mediator, signalRService)
        {

            _plexAuthenticationService = plexAuthenticationService;
            _fileMerger = fileMerger;
            _userSettings = userSettings;
            _notificationsService = notificationsService;
            _plexDownloadClientFactory = plexDownloadClientFactory;
            _downloadQueue = downloadQueue;

            _fileMerger.FileMergeProgressObservable.Subscribe(OnFileMergeProgress);
        }

        #endregion

        #region Methods

        #region Private

        private async Task<Result<DownloadTask>> AuthenticateDownloadTask(DownloadTask downloadTask)
        {
            var tokenResult = await _plexAuthenticationService.GetPlexServerTokenAsync(downloadTask.PlexServerId);
            if (tokenResult.IsFailed)
            {
                return tokenResult.ToResult();
            }

            downloadTask.ServerToken = tokenResult.Value;
            downloadTask.ServerToken = tokenResult.Value;

            return Result.Ok(downloadTask);
        }

        private async Task<Result<PlexDownloadClient>> CreateDownloadClientAsync(DownloadTask downloadTask)
        {
            if (downloadTask == null)
            {
                return ResultExtensions.IsNull(nameof(downloadTask)).LogError();
            }

            var authenticateDownloadTaskResult = await AuthenticateDownloadTask(downloadTask);
            if (authenticateDownloadTaskResult.IsFailed)
            {
                return authenticateDownloadTaskResult.ToResult().LogError();
            }

            // Create download client
            var newClient = _plexDownloadClientFactory(downloadTask);
            newClient.Parts = _userSettings.AdvancedSettings.DownloadManager.DownloadSegments;


            // Setup the client
            var setupResult = await newClient.SetupAsync(downloadTask.DownloadWorkerTasks);
            if (setupResult.IsFailed)
            {
                return setupResult.ToResult();
            }

            SetupSubscriptions(newClient);
            _downloadsList.Add(newClient);
            return Result.Ok(newClient);
        }



        /// <summary>
        /// Checks if a <see cref="DownloadTask"/> with this Id or ratingKey has already been added.
        /// </summary>
        /// <param name="downloadTask">The <see cref="DownloadTask"/> to check for.</param>
        /// <returns>True if it already exists.</returns>
        private async Task<Result<bool>> DownloadTaskExistsAsync(DownloadTask downloadTask)
        {
            if (downloadTask == null)
            {
                return ResultExtensions.IsNull(nameof(downloadTask)).LogError();
            }

            Result<DownloadTask> downloadTaskDB = null;

            // Download tasks added here might not contain an Id, which is why we also search on ratingKey.
            if (downloadTask.Id > 0)
            {
                // First check if there is an downloadClient with that downloadTask, as that is faster
                var downloadClient = _downloadsList.Find(x => x.DownloadTaskId == downloadTask.Id);
                if (downloadClient != null)
                {
                    return Result.Ok(true);
                }

                // Check Database
                downloadTaskDB = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTask.Id));
            }
            else if (downloadTask.RatingKey > 0)
            {
                // First check if there is an downloadClient with that downloadTask, as that is faster
                var downloadClient = _downloadsList.Find(x => x.DownloadTask.RatingKey == downloadTask.RatingKey);
                if (downloadClient != null)
                {
                    return Result.Ok(true);
                }

                // Check DataBase
                downloadTaskDB = await _mediator.Send(new GetDownloadTaskByRatingKeyQuery(downloadTask.RatingKey));
            }
            else
            {
                return Result.Fail("There was no valid Id or RatingKey available in the downloadTask").LogError();
            }

            if (downloadTaskDB.IsFailed)
            {
                if (downloadTaskDB.Has404NotFoundError())
                {
                    return Result.Ok(false);
                }

                return downloadTaskDB.ToResult();
            }

            // The only possibility now is that the DownloadTask exists
            return Result.Ok(true);
        }

        /// <summary>
        /// Check if a <see cref="PlexDownloadClient"/> has already been assigned to this <see cref="DownloadTask"/>.
        /// </summary>
        /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/>.</param>
        /// <returns>Returns the <see cref="PlexDownloadClient"/> if found and fails otherwise.</returns>
        private Result<PlexDownloadClient> GetDownloadClient(int downloadTaskId)
        {
            var downloadClient = _downloadsList.Find(x => x.DownloadTaskId == downloadTaskId);
            if (downloadClient == null)
            {
                return ResultExtensions
                    .Create404NotFoundResult($"There is no DownloadClient currently working on a downloadTask with Id {downloadTaskId}");
            }

            return Result.Ok(downloadClient);
        }

        private async Task CleanUpDownloadClient(int downloadTaskId)
        {
            Log.Debug($"Cleaning-up downloadClient with id {downloadTaskId}");
            var index = _downloadsList.FindIndex(x => x.DownloadTaskId == downloadTaskId);
            if (index == -1)
            {
                Log.Debug($"Could not find downloadTask with id {downloadTaskId} to clean-up.");
                return;
            }

            await _downloadsList[index].Stop();
            _downloadsList[index].Dispose();
            _downloadsList.RemoveAt(index);

            Log.Debug($"Cleaned-up PlexDownloadClient with id {downloadTaskId} from the DownloadManager");
        }

        #region Subscriptions

        private void SetupSubscriptions(PlexDownloadClient newClient)
        {
            // Download Progress Changed subscription
            newClient.DownloadProgressChanged
                .TakeUntil(newClient.DownloadFileCompleted)
                .Subscribe(OnDownloadProgressChanged);

            // Download Status Changed subscription
            newClient.DownloadStatusChanged
                .Subscribe(OnDownloadStatusChanged);

            // Download Status Changed subscription
            newClient.DownloadWorkerTaskChanged
                .Subscribe(OnDownloadWorkerTaskChanged);

            // Download File Completed subscription
            newClient.DownloadFileCompleted
                .Take(1)
                .Subscribe(OnDownloadFileCompleted);
        }

        private void OnDownloadWorkerTaskChanged(IList<DownloadWorkerTask> taskList)
        {
            Task.Run(() => _mediator.Send(new UpdateDownloadWorkerTasksCommand(taskList)));
        }

        private void OnDownloadFileCompleted(DownloadComplete downloadComplete)
        {
            Task.Run(async () =>
            {
                await _mediator.Send(new UpdateDownloadCompleteOfDownloadTaskCommand(
                    downloadComplete.Id,
                    downloadComplete.DataReceived,
                    downloadComplete.DataTotal));

                await _fileMerger.AddFileTask(downloadComplete.DownloadTask);
                await SetDownloadStatusAsync(downloadComplete.Id, DownloadStatus.Merging);
                Log.Information($"The download of {downloadComplete.DownloadTask.Title} has completed!");
                CheckDownloadQueue();
            });
        }

        private void OnDownloadStatusChanged(DownloadStatusChanged downloadStatusChanged)
        {
            SetDownloadStatus(downloadStatusChanged.Id, downloadStatusChanged.Status);
        }

        private void OnDownloadProgressChanged(DownloadProgress downloadProgress)
        {
            _signalRService.SendDownloadProgressUpdate(downloadProgress);
        }

        private void OnFileMergeProgress(FileMergeProgress progress)
        {
            Log.Debug(
                $"Merge Progress: {progress.DataTransferred} / {progress.DataTotal} - {progress.Percentage} - {progress.TransferSpeedFormatted}");
            _signalRService.SendFileMergeProgressUpdate(progress);
            if (progress.Percentage >= 100)
            {
                SetDownloadStatus(progress.DownloadTaskId, DownloadStatus.Completed);
            }
        }

        #endregion

        #endregion

        #region Public

        #region Commands

        public Result<bool> Setup()
        {
            Log.Information("Running DownloadManager setup.");
            _downloadQueue.Setup();
            return Result.Ok(true);
        }

        /// <inheritdoc/>
        public async Task<Result> DeleteDownloadClient(int downloadTaskId)
        {
            CleanUpDownloadClient(downloadTaskId);
            return await _mediator.Send(new DeleteDownloadTaskByIdCommand(downloadTaskId));
        }

        /// <inheritdoc/>
        public async Task<Result> DeleteDownloadClients(IEnumerable<int> downloadTaskIds)
        {
            foreach (int downloadTaskId in downloadTaskIds)
            {
                var result = await DeleteDownloadClient(downloadTaskId);
                if (result.IsFailed)
                {
                    return result;
                }
            }

            return Result.Ok();
        }

        /// <inheritdoc/>
        public async Task<Result<bool>> AddToDownloadQueueAsync(List<DownloadTask> downloadTasks)
        {
            Log.Debug($"Attempt to add {downloadTasks.Count} DownloadTasks");
            var downloadTasksResult = ValidateDownloadTasks(downloadTasks);
            if (downloadTasksResult.IsFailed)
            {
                return downloadTasksResult.ToResult();
            }

            // Add to Database
            var createResult = await _mediator.Send(new CreateDownloadTasksCommand(downloadTasks));
            if (createResult.IsFailed)
            {
                return createResult.ToResult().LogError();
            }

            if (createResult.Value.Count != downloadTasks.Count)
            {
                return Result.Fail("The added download tasks are not stored correctly, missing download tasks.").LogError();
            }

            // Set the Id's of the just added downloadTasks
            for (int i = 0; i < downloadTasks.Count; i++)
            {
                downloadTasks[i].Id = createResult.Value[i];
            }

            Log.Debug($"Successfully added all {downloadTasks.Count} DownloadTasks");
            CheckDownloadQueue();
            return Result.Ok(true);
        }

        /// <summary>
        /// Validates the <see cref="DownloadTask"/>s and returns only the valid one while notifying of any failed ones.
        /// Returns only a failed result when all downloadTasks failed validation.
        /// </summary>
        /// <param name="downloadTasks">The <see cref="DownloadTask"/>s to validate.</param>
        /// <returns>Only the valid <see cref="DownloadTask"/>s.</returns>
        private Result<List<DownloadTask>> ValidateDownloadTasks(List<DownloadTask> downloadTasks)
        {
            var failedList = new List<DownloadTask>();
            var result = Result.Fail("Failed to add the following DownloadTasks");
            foreach (var downloadTask in downloadTasks)
            {
                // Check validity
                var validationResult = downloadTask.IsValid();
                if (validationResult.IsFailed)
                {
                    failedList.Add(downloadTask);
                    result = Result.Merge(
                        result,
                        validationResult
                            .WithError(new Error(downloadTask.Title)
                                .WithMetadata("downloadTask", downloadTask)));
                    result.LogError();
                }

                // TODO Need a different way to check for duplicate, media consisting of multiple parts have the same rating key
                // Check if this DownloadTask is a duplicate
                // var downloadTaskExists = await DownloadTaskExistsAsync(downloadTask);
                // if (downloadTaskExists.IsFailed)
                // {
                //     // If it fails then there are bigger problems..
                //     return downloadTaskExists;
                // }
                //
                // if (downloadTaskExists.Value)
                // {
                //     failedList.Add(downloadTask);
                // }
            }

            // All download tasks failed validation
            if (failedList.Count == downloadTasks.Count)
            {
                return result;
            }

            // Some failed, alert front-end of some failing
            if (failedList.Count > 0)
            {
                _notificationsService.SendResult(result);
                return Result.Ok(downloadTasks.Except(failedList).ToList());
            }

            // All passed
            return Result.Ok(downloadTasks);
        }

        /// <summary>
        /// Check the DownloadQueue for downloadTasks which can be started.
        /// </summary>
        public void CheckDownloadQueue()
        {
            _downloadQueue.CheckDownloadQueue(this);
        }

        /// <param name="downloadTaskIds"></param>
        /// <inheritdoc/>
        public async Task<Result<bool>> ClearCompletedAsync(List<int> downloadTaskIds = null)
        {
            return await _mediator.Send(new ClearCompletedDownloadTasksCommand(downloadTaskIds));
        }

        /// <inheritdoc/>
        public async Task<Result<bool>> RestartDownloadAsync(int downloadTaskId)
        {
            return await StartDownload(downloadTaskId);
        }

        /// <inheritdoc/>
        public async Task<Result> StopDownload(List<int> downloadTaskIds = null)
        {
            if (downloadTaskIds == null)
            {
                return ResultExtensions.IsNull(nameof(downloadTaskIds));
            }

            // Retrieve download client
            foreach (int downloadTaskId in downloadTaskIds)
            {
                var downloadClient = GetDownloadClient(downloadTaskId);
                if (downloadClient.IsSuccess)
                {
                    await downloadClient.Value.Stop();
                }
            }

            return Result.Ok();
        }

        /// <inheritdoc/>
        public async Task<Result<bool>> StartDownload(int downloadTaskId)
        {
            CleanUpDownloadClient(downloadTaskId);

            // Client does not exist yet, create one
            var downloadTask = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId, true, true));
            if (downloadTask.IsFailed)
            {
                return downloadTask.ToResult();
            }

            // TODO check if there is already a download in progress on the same server

            var downloadClient = await CreateDownloadClientAsync(downloadTask.Value);
            if (downloadClient.IsFailed)
            {
                return downloadClient.ToResult();
            }

            return await downloadClient.Value.Start();
        }

        /// <inheritdoc/>
        public async Task<Result> PauseDownload(int downloadTaskId)
        {
            // Retrieve download client
            var downloadClient = GetDownloadClient(downloadTaskId);
            if (downloadClient.IsSuccess && downloadClient.Value != null)
            {
                return await downloadClient.Value.Pause();
            }

            return downloadClient
                .WithError($"DownloadTask with id {downloadTaskId} is not currently downloading")
                .ToResult()
                .LogWarning();
        }

        #endregion

        #endregion

        #endregion
    }
}