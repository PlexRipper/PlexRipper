using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexDownloads.Commands;
using PlexRipper.Application.PlexDownloads.Queries;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Common;
using PlexRipper.DownloadManager.Download;

namespace PlexRipper.DownloadManager
{
    /// <summary>
    /// Handles all <see cref="DownloadTask"/> management, all download related commands should be handled here.
    /// </summary>
    public class DownloadManager : IDownloadManager
    {
        #region Fields

        private readonly IMediator _mediator;
        private readonly ISignalRService _signalRService;
        private readonly IPlexAuthenticationService _plexAuthenticationService;
        private readonly IFileSystem _fileSystem;
        private readonly IFileManager _fileManager;

        /// <summary>
        /// Currently loaded and active <see cref="PlexDownloadClient"/>s.
        /// </summary>
        private readonly List<PlexDownloadClient> _downloadsList = new List<PlexDownloadClient>();

        private readonly DownloadQueue _downloadQueue;

        private Task<Task> _checkDownloadTask;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadManager"/> class.
        /// </summary>
        /// <param name="mediator">Defines a mediator to encapsulate request/response and publishing interaction patterns.</param>
        /// <param name="signalRService"></param>
        /// <param name="plexAuthenticationService">.</param>
        /// <param name="fileSystem">.</param>
        /// <param name="fileManager">.</param>
        /// <param name="downloadQueue">Used to retrieve the next <see cref="DownloadTask"/> from the <see cref="DownloadQueue"/>.</param>
        public DownloadManager(
            IMediator mediator,
            ISignalRService signalRService,
            IPlexAuthenticationService plexAuthenticationService,
            IFileSystem fileSystem,
            IFileManager fileManager,
            DownloadQueue downloadQueue)
        {
            _mediator = mediator;
            _signalRService = signalRService;
            _plexAuthenticationService = plexAuthenticationService;
            _fileSystem = fileSystem;
            _fileManager = fileManager;
            _downloadQueue = downloadQueue;
            System.Net.ServicePointManager.DefaultConnectionLimit = 1000;

            _fileManager.FileMergeProgressObservable.Subscribe(OnFileMergeProgress);
        }

        #endregion Constructors

        #region Methods

        #region Commands

        /// <summary>
        /// Adds a single DownloadTask to the Download queue.
        /// </summary>
        /// <param name="downloadTask">The <see cref="DownloadTask"/> that will be checked and added.</param>
        /// <param name="performCheck">Should the CheckDownloadQueue() be called at the end.</param>
        /// <returns>Returns true if successfully added and false if the downloadTask already exists.</returns>
        public async Task<Result<bool>> AddToDownloadQueueAsync(DownloadTask downloadTask, bool performCheck = true)
        {
            // Download tasks added here do not contain an Id since they have not been added to the database yet.
            if (downloadTask == null)
            {
                return ResultExtensions.IsNull(nameof(downloadTask)).LogError();
            }

            if (!downloadTask.DownloadUri.IsAbsoluteUri)
            {
                return Result.Fail(
                    new Error($"The url {downloadTask.DownloadUri} is not absolute and thus not valid."));
            }

            if (!Uri.TryCreate(downloadTask.DownloadUri.ToString(), UriKind.Absolute, out Uri outUri)
                && !(outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
            {
                return Result.Fail(
                    new Error($"The uri {downloadTask.DownloadUri} is not valid.")
                        .WithMetadata("Uri", downloadTask.DownloadUri));
            }

            // TODO Re-enable checking for existing download task after testing
            var downloadTaskExists = await DownloadTaskExistsAsync(downloadTask);
            if (downloadTaskExists.IsFailed)
            {
                return downloadTaskExists;
            }

            if (downloadTaskExists.Value)
            {
                return Result.Fail($"DownloadTask with id: {downloadTask.Id} or ratingKey: {downloadTask.RatingKey} already exists").LogError();
            }

            // Add to Database
            Log.Debug($"Adding new downloadTask: {downloadTask.Title} with ratingKey: {downloadTask.RatingKey}");
            var result = await _mediator.Send(new AddDownloadTaskCommand(downloadTask));
            if (result.IsFailed)
            {
                return result.ToResult().LogError();
            }

            if (performCheck)
            {
                CheckDownloadQueue();
            }

            return Result.Ok(true);
        }

        /// <summary>
        /// Adds a list of <see cref="DownloadTask"/>s to the download queue.
        /// </summary>
        /// <param name="downloadTasks">The list of <see cref="DownloadTask"/>s that will be checked and added.</param>
        /// <returns>Returns true if all downloadTasks were added successfully.</returns>
        public async Task<Result<bool>> AddToDownloadQueueAsync(List<DownloadTask> downloadTasks)
        {
            Log.Debug($"Attempt to add {downloadTasks.Count} DownloadTasks");
            var failedList = new List<DownloadTask>();
            foreach (var downloadTask in downloadTasks)
            {
                var result = await AddToDownloadQueueAsync(downloadTask, false);
                if (result.IsFailed || !result.Value)
                {
                    failedList.Add(downloadTask);
                }
            }

            if (failedList.Count > 0)
            {
                var result = new Result();
                var error = new Error();
                foreach (var downloadTask in failedList)
                {
                    error.Reasons.Add(new Error("Download task failed to be added to the downloadQueue")
                        .WithMetadata("downloadTask", downloadTask));
                }

                return Result.Fail(error).Add400BadRequestError().LogError();
            }

            Log.Debug($"Successfully added all {downloadTasks.Count} DownloadTasks");
            CheckDownloadQueue();
            return Result.Ok(true);
        }

        /// <summary>
        /// Check the DownloadQueue for downloadTasks which can be started.
        /// </summary>
        public void CheckDownloadQueue()
        {
            Log.Debug("Executing download queue check!");
            if (_checkDownloadTask?.Status.Equals(TaskStatus.Running) ?? false)
            {
                Log.Warning("Check download Queue already in progress");
            }

            _checkDownloadTask = Task.Factory.StartNew(
                async () =>
                {
                    Log.Debug("Checking for download tasks which can be processed.");
                    var serverListResult = await _mediator.Send(new GetAllDownloadTasksInPlexServersQuery(true, true));
                    var serverList = serverListResult.Value.Where(x => x.HasDownloadTasks).ToList();

                    Log.Information($"Starting the check of {serverList.Count} PlexServers.");
                    if (serverList.Any())
                    {
                        foreach (var server in serverList)
                        {
                            var downloadTask = await _downloadQueue.NextDownloadAsync(server);

                            if (downloadTask.IsFailed)
                            {
                                continue;
                            }

                            // Check if there is already a client working this downloadTask
                            var downloadClient = GetDownloadClient(downloadTask.Value.Id);
                            if (downloadClient.IsFailed)
                            {
                                downloadClient = await CreateDownloadClientAsync(downloadTask.Value);
                                downloadClient.Value.Start();
                            }
                        }
                    }
                    else
                    {
                        Log.Information("There are no PlexServers with DownloadTasks");
                    }
                }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Will clear any completed <see cref="DownloadTask"/> from the database.
        /// </summary>
        /// <returns>Is successful.</returns>
        public async Task<Result<bool>> ClearCompletedAsync()
        {
            return await _mediator.Send(new ClearCompletedDownloadTasksCommand());
        }

        /// <summary>
        /// Restart the <see cref="DownloadTask"/> by deleting the <see cref="PlexDownloadClient"/> and starting a new one.
        /// </summary>
        /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/> to restart.</param>
        /// <returns>Is successful.</returns>
        public async Task<Result<bool>> RestartDownloadAsync(int downloadTaskId)
        {
            DeleteDownloadClient(downloadTaskId);

            return await StartDownload(downloadTaskId);
        }

        /// <summary>
        /// Cancels the <see cref="PlexDownloadClient"/> executing the <see cref="DownloadTask"/> if it is downloading.
        /// </summary>
        /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/> to stop.</param>
        /// <returns>Is successful.</returns>
        public async Task<Result<bool>> StopDownloadAsync(int downloadTaskId)
        {
            // Retrieve download client
            var downloadClient = GetDownloadClient(downloadTaskId);
            if (downloadClient.IsSuccess)
            {
                downloadClient.Value.Stop();
            }

            await SetDownloadStatusAsync(new DownloadStatusChanged(downloadTaskId, DownloadStatus.Stopped));
            return Result.Ok(true);
        }

        /// <summary>
        /// Starts a queued task immediately.
        /// </summary>
        /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/> to start.</param>
        /// <returns>Is successful.</returns>
        public async Task<Result<bool>> StartDownload(int downloadTaskId)
        {
            DeleteDownloadClient(downloadTaskId);

            // Client does not exist yet, create one
            var downloadTask = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId, true, true, true));
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

            return downloadClient.Value.Start();
        }

        /// <summary>
        /// Pause a currently downloading <see cref="DownloadTask"/>.
        /// </summary>
        /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/> to pause.</param>
        /// <returns>Is successful.</returns>
        public Result<bool> PauseDownload(int downloadTaskId)
        {
            // Retrieve download client
            var downloadClient = GetDownloadClient(downloadTaskId);
            if (downloadClient.IsSuccess && downloadClient.Value != null)
            {
                return downloadClient.Value.Pause();
            }

            return downloadClient
                .WithError($"DownloadTask with id {downloadTaskId} is not currently downloading")
                .ToResult()
                .LogWarning();
        }

        #endregion

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

                await _fileManager.AddFileTask(downloadComplete.DownloadTask);
                await SetDownloadStatusAsync(new DownloadStatusChanged(downloadComplete.Id, DownloadStatus.Merging));
                DeleteDownloadClient(downloadComplete.Id);
                Log.Information($"The download of {downloadComplete.DownloadTask.Title} has completed!");
                CheckDownloadQueue();
            });
        }

        private void OnDownloadStatusChanged(DownloadStatusChanged downloadStatusChanged)
        {
            Task.Run(async () => { await SetDownloadStatusAsync(downloadStatusChanged); });
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
                Task.Run(async () => { await SetDownloadStatusAsync(new DownloadStatusChanged(progress.DownloadTaskId, DownloadStatus.Completed)); });
            }
        }

        #endregion

        private async Task<Result<PlexDownloadClient>> CreateDownloadClientAsync(DownloadTask downloadTask)
        {
            if (downloadTask == null)
            {
                return ResultExtensions.IsNull(nameof(downloadTask)).LogError();
            }

            var downloadUrl = await _plexAuthenticationService.GetPlexServerTokenWithUrl(
                downloadTask.PlexAccountId,
                downloadTask.PlexServerId,
                downloadTask.DownloadUrl);

            if (downloadUrl.IsFailed)
            {
                return downloadUrl.ToResult();
            }

            // Determine media size
            // Source: https://stackoverflow.com/a/48190014/8205497
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(downloadUrl.Value, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Failed to create the downloadClient for url {downloadUrl.Value}")
                    .WithMessage(response.ReasonPhrase)).LogError();
            }

            var newDataTotal = response.Content.Headers.ContentLength ?? -1L;
            if (downloadTask.DataTotal > 0 && downloadTask.DataTotal != newDataTotal)
            {
                // The media size changes, re-create download workers and delete any old ones.
                downloadTask.DownloadWorkerTasks = null;
                await _mediator.Send(new DeleteDownloadWorkerTasksByDownloadTaskIdCommand(downloadTask.Id));
            }

            downloadTask.DataTotal = newDataTotal;

            // Update downloadTask row in database
            var result = await _mediator.Send(new UpdateDownloadTaskByIdCommand(downloadTask));
            if (result.IsFailed)
            {
                return result.ToResult().LogError();
            }

            // Retrieve latest downloadTask row in database
            var downloadTaskDb = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTask.Id, true, true, true));
            if (downloadTaskDb.IsFailed)
            {
                return result.ToResult().LogError();
            }

            downloadTask = downloadTaskDb.Value;

            // Create download client
            var newClient = new PlexDownloadClient(downloadTask, _mediator, _fileSystem)
            {
                DownloadUrl = downloadUrl.Value,

                // TODO Make this configurable from the front-end
                Parts = 4,
            };

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

                // Check DataBase
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

        private async Task<Result> SetDownloadStatusAsync(DownloadStatusChanged downloadStatusChanged)
        {
            Log.Debug($"DownloadClient changed downloadStatus for downloadTask {downloadStatusChanged.Id} " +
                      $"to {downloadStatusChanged.Status.ToString()}");

            var result = await _mediator.Send(new UpdateDownloadStatusOfDownloadTaskCommand(downloadStatusChanged.Id, downloadStatusChanged.Status));
            if (result.IsFailed)
            {
                return result;
            }

            await _signalRService.SendDownloadStatusUpdate(downloadStatusChanged.Id, downloadStatusChanged.Status);
            return Result.Ok();
        }

        /// <summary>
        /// Deletes the <see cref="PlexDownloadClient"/> from the _downloadList and executes its disposal.
        /// </summary>
        /// <param name="downloadTaskId">The id of <see cref="PlexDownloadClient"/> to delete,
        /// the <see cref="DownloadTask"/> id can be used as these are always the same.</param>
        public void DeleteDownloadClient(int downloadTaskId)
        {
            Log.Debug($"Cleaning-up downloadClient with id {downloadTaskId}");
            var index = _downloadsList.FindIndex(x => x.DownloadTaskId == downloadTaskId);
            if (index > -1)
            {
                _downloadsList[index].Dispose();
                _downloadsList.RemoveAt(index);
                Log.Debug($"Cleaned-up downloadClient with id {downloadTaskId}");
            }
            else
            {
                Log.Warning($"Download client with Id does not exist and could therefore not be deleted.");
            }
        }

        #endregion Methods
    }
}