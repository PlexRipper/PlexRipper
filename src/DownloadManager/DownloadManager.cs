using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexDownloads;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Download;

namespace PlexRipper.DownloadManager
{
    /// <summary>
    /// Handles all <see cref="DownloadTask"/> management, all download related commands should be handled here.
    /// </summary>
    public class DownloadManager : IDownloadManager
    {
        #region Fields

        /// <summary>
        /// Currently loaded and active <see cref="PlexDownloadClient"/>s.
        /// </summary>
        private readonly List<PlexDownloadClient> _downloadsList = new();

        private readonly IFileMerger _fileMerger;

        private readonly IMediator _mediator;

        private readonly ISignalRService _signalRService;

        private readonly IPlexAuthenticationService _plexAuthenticationService;

        private readonly IFileSystemCustom _fileSystem;

        private readonly INotificationsService _notificationsService;

        private readonly IPlexRipperHttpClient _httpClient;

        private readonly IDownloadQueue _downloadQueue;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadManager"/> class.
        /// </summary>
        /// <param name="mediator">Defines a mediator to encapsulate request/response and publishing interaction patterns.</param>
        /// <param name="signalRService"></param>
        /// <param name="plexAuthenticationService">.</param>
        /// <param name="fileSystem"></param>
        /// <param name="fileMerger">.</param>
        /// <param name="downloadQueue">Used to retrieve the next <see cref="DownloadTask"/> from the <see cref="DownloadQueue"/>.</param>
        /// <param name="notificationsService"></param>
        /// <param name="httpClient"></param>
        public DownloadManager(
            IMediator mediator,
            ISignalRService signalRService,
            IPlexAuthenticationService plexAuthenticationService,
            IFileSystemCustom fileSystem,
            IFileMerger fileMerger,
            IDownloadQueue downloadQueue,
            INotificationsService notificationsService,
            IPlexRipperHttpClient httpClient)
        {
            _mediator = mediator;
            _signalRService = signalRService;
            _plexAuthenticationService = plexAuthenticationService;
            _fileSystem = fileSystem;
            _fileMerger = fileMerger;
            _notificationsService = notificationsService;
            _httpClient = httpClient;
            _downloadQueue = downloadQueue;

            _fileMerger.FileMergeProgressObservable.Subscribe(OnFileMergeProgress);

            Log.Information("Running DownloadManager setup.");

            // Setup DownloadQueue subscriptions
            _downloadQueue.StartDownloadTask
                .Select(id => Observable.FromAsync(async () => await StartDownload(id)))
                .Concat()
                .Subscribe();

            _downloadQueue.UpdateDownloadClient
                .Select(update => Observable.FromAsync(async () => await UpdateDownloadTaskStatusAsync(update)))
                .Concat()
                .Subscribe();
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
            var newClient = PlexDownloadClient.Create(downloadTask, _fileSystem, _httpClient);
            if (newClient.IsFailed)
            {
                return newClient.ToResult().LogError();
            }

            var validateDownloadSizeResult = await ValidateDownloadSize(downloadTask);
            if (validateDownloadSizeResult.IsFailed)
            {
                return validateDownloadSizeResult.LogError();
            }

            SetupSubscriptions(newClient.Value);
            _downloadsList.Add(newClient.Value);
            return Result.Ok(newClient.Value);
        }

        private async Task<Result> ValidateDownloadSize(DownloadTask downloadTask)
        {
            // Determine media size
            // Source: https://stackoverflow.com/a/48190014/8205497
            var response = await _httpClient.GetAsync(downloadTask.DownloadUri, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail($"The {nameof(ValidateDownloadSize)} returned a invalid status code of {response.StatusCode}");
            }

            var newDataTotal = response.Content.Headers.ContentLength ?? -1L;
            if (downloadTask.DataTotal > 0 && downloadTask.DataTotal != newDataTotal)
            {
                //TODO Implement recreation of downloadTask when downloadSize is a mismatch, this indicates the media has been updated.
                // The media size changes, re-create download workers and delete any old ones.
                downloadTask.DownloadWorkerTasks = null;
                await _mediator.Send(new DeleteDownloadWorkerTasksByDownloadTaskIdCommand(downloadTask.Id));
                return Result.Fail(
                    $"The downloadTask has an incorrect total download size, has {downloadTask.DataTotal} and should be {newDataTotal}");
            }

            return Result.Ok();
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

            Result<DownloadTask> downloadTaskDB;

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
            else if (downloadTask.Key > 0)
            {
                // First check if there is an downloadClient with that downloadTask, as that is faster
                var downloadClient = _downloadsList.Find(x => x.DownloadTask.Key == downloadTask.Key);
                if (downloadClient != null)
                {
                    return Result.Ok(true);
                }

                // Check DataBase
                downloadTaskDB = await _mediator.Send(new GetDownloadTaskByRatingKeyQuery(downloadTask.Key));
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

        /// <summary>
        /// Deletes and disposes the <see cref="PlexDownloadClient"/> from the DownloadMa
        /// </summary>
        /// <param name="downloadTaskId"></param>
        private async Task CleanUpDownloadClient(int downloadTaskId)
        {
            Log.Debug($"Cleaning-up downloadClient with id {downloadTaskId}");
            var index = _downloadsList.FindIndex(x => x.DownloadTaskId == downloadTaskId);
            if (index == -1)
            {
                Log.Debug($"Could not find downloadTask with id {downloadTaskId} to clean-up.");
                return;
            }

            if (_downloadsList[index] is not null)
            {
                await _downloadsList[index].StopAsync();
                _downloadsList[index].Dispose();
                _downloadsList.RemoveAt(index);
            }

            Log.Debug($"Cleaned-up PlexDownloadClient with id {downloadTaskId} from the DownloadManager");
        }

        private void RemoveDownloadTempFiles(DownloadTask downloadTask)
        {
            foreach (var workerTask in downloadTask.DownloadWorkerTasks)
            {
                _fileSystem.DeleteAllFilesFromDirectory(workerTask.TempDirectory);
            }
        }

        #region Subscriptions

        private void SetupSubscriptions(PlexDownloadClient newClient)
        {
            // Download client update subscription
            newClient.DownloadClientUpdate
                .Select(update => Observable.FromAsync(async () => await UpdateDownloadTaskStatusAsync(update)))
                .Concat()
                .Subscribe();

            // Download Worker Task completed subscription
            newClient.DownloadClientUpdate
                .Where(x => x.DownloadStatus == DownloadStatus.Completed)
                .Select(update => Observable.FromAsync(async () => await OnDownloadFileCompleted(update)))
                .Concat()
                .Subscribe();

            // Download Worker Log subscription
            newClient.DownloadWorkerLog
                .Buffer(TimeSpan.FromSeconds(1))
                .Select(logs => Observable.FromAsync(async () => await _mediator.Send(new AddDownloadWorkerLogsCommand(logs))))
                .Concat()
                .Subscribe();
        }

        private async Task OnDownloadFileCompleted(DownloadClientUpdate downloadClientUpdate)
        {
            var downloadTask = downloadClientUpdate.DownloadTask;
            downloadTask.DownloadStatus = DownloadStatus.Merging;
            downloadTask.DownloadWorkerTasks.ForEach(x => x.DownloadStatus = DownloadStatus.Merging);

            await _fileMerger.AddFileTask(downloadTask);
            await UpdateDownloadTaskStatusAsync(downloadClientUpdate);

            Log.Information($"The download of {downloadTask.Title} has completed!");
            await CheckDownloadQueue();
        }

        private void OnFileMergeProgress(FileMergeProgress progress)
        {
            Log.Debug(
                $"Merge Progress: {progress.DataTransferred} / {progress.DataTotal} - {progress.Percentage} - {progress.TransferSpeedFormatted}");
            _signalRService.SendFileMergeProgressUpdate(progress);
            if (progress.Percentage >= 100)
            {
                Task.Run(async () =>
                {
                    var downloadTaskResultDownloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(progress.DownloadTaskId));
                    if (downloadTaskResultDownloadTaskResult.IsFailed)
                    {
                        downloadTaskResultDownloadTaskResult.LogError();
                        return;
                    }

                    downloadTaskResultDownloadTaskResult.Value.DownloadStatus = DownloadStatus.Completed;
                    await UpdateDownloadTaskStatusAsync(new DownloadClientUpdate(downloadTaskResultDownloadTaskResult.Value));
                });
            }
        }

        private async Task UpdateDownloadTaskStatusAsync(DownloadClientUpdate downloadClientUpdate)
        {
            Log.Debug($"DownloadClient changed downloadStatus for downloadTask {downloadClientUpdate.Id} " +
                      $"to {downloadClientUpdate.DownloadStatus.ToString()}");

            await _mediator.Send(new UpdateDownloadTaskByIdCommand(downloadClientUpdate.DownloadTask));
            await _signalRService.SendDownloadTaskUpdate(downloadClientUpdate);
        }

        #endregion

        #endregion

        #region Public

        #region Commands

        /// <inheritdoc/>
        public async Task<Result> DeleteDownloadClient(int downloadTaskId)
        {
            await CleanUpDownloadClient(downloadTaskId);
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
        public async Task<Result> AddToDownloadQueueAsync(List<DownloadTask> downloadTasks)
        {
            if (downloadTasks is null || !downloadTasks.Any())
            {
                return Result.Fail("Parameter downloadTasks was empty or null").LogError();
            }

            Log.Debug($"Attempt to add {downloadTasks.Count} DownloadTasks");
            var downloadTasksResult = ValidateDownloadTasks(downloadTasks);
            if (downloadTasksResult.IsFailed)
            {
                return downloadTasksResult.ToResult();
            }

            // Add to Database
            var createResult = await _mediator.Send(new CreateDownloadTasksCommand(downloadTasksResult.Value));
            if (createResult.IsFailed)
            {
                return createResult.ToResult().LogError();
            }

            if (createResult.Value.Count != downloadTasksResult.Value.Count)
            {
                return Result.Fail("The added download tasks are not stored correctly, missing download tasks.").LogError();
            }

            Log.Debug($"Successfully added all {downloadTasksResult.Value.Count} DownloadTasks");
            await CheckDownloadQueue();
            return Result.Ok();
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
            for (int i = 0; i < downloadTasks.Count; i++)
            {
                var downloadTask = downloadTasks[i];

                // Check validity
                Log.Debug($"Checking DownloadTask {i + 1} of {downloadTasks.Count} with title {downloadTask.TitlePath}");
                var validationResult = downloadTask.IsValid();
                if (validationResult.IsFailed)
                {
                    validationResult.LogError();
                    failedList.Add(downloadTask);
                    validationResult.Errors.ForEach(x => x.WithMetadata("downloadTask Title", downloadTask.TitlePath));
                    result.AddNestedErrors(validationResult.Errors);
                }
                else
                {
                    Log.Information($"DownloadTask {i + 1} of {downloadTasks.Count} with title {downloadTask.TitlePath} was valid");
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
        public async Task CheckDownloadQueue()
        {
            Log.Debug("Checking for download tasks which can be processed.");
            var serverListResult = await _mediator.Send(new GetAllDownloadTasksInPlexServersQuery(true));
            var serverList = serverListResult.Value.Where(x => x.HasDownloadTasks).ToList();

            _downloadQueue.ExecuteDownloadQueue(serverList);
        }

        /// <param name="downloadTaskIds"></param>
        /// <inheritdoc/>
        public async Task<Result> ClearCompletedAsync(List<int> downloadTaskIds)
        {
            return await _mediator.Send(new ClearCompletedDownloadTasksCommand(downloadTaskIds));
        }

        /// <inheritdoc/>
        public async Task<Result> RestartDownloadAsync(List<int> downloadTaskIds)
        {
            if (downloadTaskIds == null)
            {
                return ResultExtensions.IsNull(nameof(downloadTaskIds));
            }

            // Retrieve download client
            foreach (int downloadTaskId in downloadTaskIds)
            {
                var downloadClient = GetDownloadClient(downloadTaskId);
                DownloadTask downloadTask = null;
                if (downloadClient.IsSuccess)
                {
                    await downloadClient.Value.StopAsync();
                    downloadTask = downloadClient.Value.DownloadTask;
                    await CleanUpDownloadClient(downloadTask.Id);
                }

                if (downloadTask is null)
                {
                    var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId, true, true));
                    if (downloadTaskResult.IsFailed)
                    {
                        continue;
                    }

                    downloadTask = downloadTaskResult.Value;
                }

                RemoveDownloadTempFiles(downloadTask);

                downloadTask.DownloadStatus = DownloadStatus.Initialized;
                await UpdateDownloadTaskStatusAsync(new DownloadClientUpdate(downloadTask));
            }

            await CheckDownloadQueue();

            return Result.Ok();
        }

        /// <inheritdoc/>
        public async Task<Result> StopDownload(List<int> downloadTaskIds)
        {
            if (downloadTaskIds == null)
            {
                return ResultExtensions.IsNull(nameof(downloadTaskIds));
            }

            // Retrieve download client
            foreach (int downloadTaskId in downloadTaskIds)
            {
                var downloadClient = GetDownloadClient(downloadTaskId);
                DownloadTask downloadTask = null;
                if (downloadClient.IsSuccess)
                {
                    await downloadClient.Value.StopAsync();
                    downloadTask = downloadClient.Value.DownloadTask;
                }

                if (downloadTask is null)
                {
                    var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId, true, true));
                    if (downloadTaskResult.IsFailed)
                    {
                        continue;
                    }

                    downloadTask = downloadTaskResult.Value;
                }

                downloadTask.DownloadStatus = DownloadStatus.Stopped;
                await UpdateDownloadTaskStatusAsync(new DownloadClientUpdate(downloadTask));
            }

            return Result.Ok();
        }

        /// <inheritdoc/>
        public async Task<Result> StartDownload(int downloadTaskId)
        {
            // Client does not exist yet, create one
            var downloadTask = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId, true, true));
            if (downloadTask.IsFailed)
            {
                return downloadTask.ToResult();
            }

            var downloadClient = await CreateDownloadClientAsync(downloadTask.Value);
            if (downloadClient.IsFailed)
            {
                return downloadClient.ToResult();
            }

            Log.Information($"Starting download of {downloadTask.Value.TitlePath}");
            return downloadClient.Value.Start();
        }

        /// <inheritdoc/>
        public async Task<Result> PauseDownload(int downloadTaskId)
        {
            // Retrieve download client

            var downloadClient = GetDownloadClient(downloadTaskId);
            if (downloadClient.IsSuccess && downloadClient.Value != null)
            {
                Log.Information($"Pausing download of {downloadClient.Value.DownloadTask.TitlePath}");
                return await downloadClient.Value.StopAsync();
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