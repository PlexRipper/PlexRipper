using FluentResults;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.FileSystem;
using PlexRipper.Application.Common.Interfaces.Settings;
using PlexRipper.Application.PlexDownloads.Commands;
using PlexRipper.Application.PlexDownloads.Queries;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Types;
using PlexRipper.DownloadManager.Common;
using PlexRipper.DownloadManager.Download;
using PlexRipper.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.SignalR;
using PlexRipper.Domain.Common;
using PlexRipper.Domain.Enums;

namespace PlexRipper.DownloadManager
{
    public class DownloadManager : IDownloadManager
    {
        #region Fields

        private readonly IMediator _mediator;
        private readonly ISignalRService _signalRService;
        private readonly IPlexAuthenticationService _plexAuthenticationService;
        private readonly IUserSettings _userSettings;
        private readonly IFileSystem _fileSystem;

        // Collection which contains all download clients, bound to the DataGrid control
        public List<PlexDownloadClient> DownloadsList = new List<PlexDownloadClient>();

        #endregion Fields

        #region Properties

        // Number of currently active downloads
        public int ActiveDownloads
        {
            get
            {
                int active = 0;

                //foreach (WebDownloadClient d in DownloadsList)
                //{
                //    if (!d.HasError)
                //        if (d.Status == DownloadStatus.Waiting || d.Status == DownloadStatus.Downloading)
                //            active++;
                //}
                return active;
            }
        }

        // Number of completed downloads
        public int CompletedDownloads
        {
            get
            {
                int completed = 0;

                //foreach (WebDownloadClient d in DownloadsList)
                //{
                //    if (d.Status == DownloadStatus.Completed)
                //        completed++;
                //}
                return completed;
            }
        }

        // Total number of downloads in the list
        public int TotalDownloads => DownloadsList.Count;

        #endregion Properties

        #region Constructors

        public DownloadManager(IMediator mediator, ISignalRService signalRService, IPlexAuthenticationService plexAuthenticationService,
            IUserSettings userSettings, IFileSystem fileSystem)
        {
            _mediator = mediator;
            _signalRService = signalRService;
            _plexAuthenticationService = plexAuthenticationService;
            _userSettings = userSettings;
            _fileSystem = fileSystem;
            SetMaxThreads();
        }

        #endregion Constructors

        #region Methods

        private async Task<Result<PlexDownloadClient>> CreateDownloadClientAsync(DownloadTask downloadTask)
        {
            PlexDownloadClient newClient = new PlexDownloadClient(downloadTask, _fileSystem);

            var token = await _plexAuthenticationService.GetPlexServerTokenAsync(downloadTask.PlexAccountId, downloadTask.PlexServerId);
            if (token.IsFailed)
            {
                return token.ToResult();
            }

            newClient.PlexServerAuthToken = token.Value;
            newClient.Parts = 8;

            newClient.DownloadProgressChanged += OnDownloadProgressChanged;

            newClient.DownloadFileCompleted += OnDownloadFileCompleted;

            newClient.DownloadStatusChanged += OnDownloadStatusChanged;

            DownloadsList.Add(newClient);
            return Result.Ok(newClient);
        }

        private async void OnDownloadStatusChanged(object sender, DownloadStatus status)
        {
            var plexDownloadClient = sender as PlexDownloadClient;
            Log.Debug($"DownloadClient changed downloadStatus for downloadTask {plexDownloadClient.DownloadTaskId} to {status.ToString()}");

            await _mediator.Send(new UpdateDownloadStatusOfDownloadTaskCommand(plexDownloadClient.DownloadTaskId, status));
        }

        private async void OnDownloadFileCompleted(object sender, DownloadProgress downloadProgress)
        {
            var plexDownloadClient = sender as PlexDownloadClient;
            Log.Information($"The download of {plexDownloadClient.DownloadTask.Title} has completed!");
            await CheckDownloadQueue();
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgress downloadProgress)
        {
            var plexDownloadClient = sender as PlexDownloadClient;

            _signalRService.SendDownloadProgressUpdate(downloadProgress);

            Log.Information(
                $"({plexDownloadClient.DownloadTaskId}){plexDownloadClient.DownloadTask.FileName} => Downloaded {DataFormat.FormatSizeString(downloadProgress.DataReceived)} of {DataFormat.FormatSizeString(downloadProgress.DataTotal)} bytes ({downloadProgress.DownloadSpeed}). {downloadProgress.Percentage} % complete...");
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
                var downloadClient = DownloadsList.Find(x => x.DownloadTaskId == downloadTask.Id);
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
                var downloadClient = DownloadsList.Find(x => x.DownloadTask.RatingKey == downloadTask.RatingKey);
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
        /// Adds a single DownloadTask to the Download queue
        /// </summary>
        /// <param name="downloadTask">The <see cref="DownloadTask"/> that will be checked and added.</param>
        /// <param name="performCheck">Should the CheckDownloadQueue() be called at the end</param>
        /// <returns>Returns true if successfully added and false if the downloadTask already exists</returns>
        public async Task<Result<bool>> AddToDownloadQueueAsync(DownloadTask downloadTask, bool performCheck = true)
        {
            // Download tasks added here do not contain an Id since they have not been added to the database yet.
            if (downloadTask == null)
            {
                return ResultExtensions.IsNull(nameof(downloadTask)).LogError();
            }


            // TODO Re-enable checking for existing download task after testing
            // var downloadTaskExists = await DownloadTaskExistsAsync(downloadTask);
            // if (downloadTaskExists.IsFailed)
            // {
            //     return downloadTaskExists;
            // }
            //
            // if (downloadTaskExists.Value)
            // {
            //     return Result.Fail($"DownloadTask with id: {downloadTask.Id} or ratingKey: {downloadTask.RatingKey} already exists").LogError();
            // }

            // Add to Database
            Log.Debug($"Adding new downloadTask: {downloadTask.Title} with ratingKey: {downloadTask.RatingKey}");
            var result = await _mediator.Send(new AddDownloadTaskCommand(downloadTask));
            if (result.IsFailed)
            {
                return result.ToResult();
            }

            if (performCheck)
            {
                await CheckDownloadQueue();
            }

            return Result.Ok(true);
        }

        /// <summary>
        /// Check the DownloadQueue for downloadTasks which can be started.
        /// </summary>
        /// <returns></returns>
        public async Task<Result> CheckDownloadQueue()
        {
            Log.Debug("Checking for download tasks which can be processed.");
            var downloadTasks = await _mediator.Send(new GetAllDownloadTasksQuery(true, true, true));
            if (!downloadTasks.Value.Any())
            {
                Log.Debug("No download tasks found to start processing.");
                return new Result();
            }

            Log.Information($"Starting the check of {downloadTasks.Value.Count} downloadTasks.");
            foreach (var downloadTask in downloadTasks.Value)
            {
                var downloadClient = GetDownloadClient(downloadTask.Id);
                if (downloadClient.Has404NotFoundError())
                {
                    downloadClient = await CreateDownloadClientAsync(downloadTask);
                    await downloadClient.Value.StartAsync();

                    // There is an downloadClient which already has this downloadTask
                    // downloadClient.Value.DownloadStatus
                }

                // Check if already added to a downloadClient
                // Check if the server of that downloadTask already has a running download task
            }

            // try
            // {
            //     // TODO This might be removed if the authToken is stored in the database.
            //     downloadTaskDB.Value.PlexServerAuthToken = downloadTask.PlexServerAuthToken;
            //
            //     Log.Debug(downloadTaskDB.Value.ToString());
            //     var downloadClient = CreateDownloadClient(downloadTaskDB.Value);
            //     return await downloadClient.StartAsync();
            // }
            // catch (Exception e)
            // {
            //     return Result.Fail($"Failed to start the Download of {downloadTask.FileName}").LogError(e);
            // }
            return Result.Ok(true);
        }

        /// <summary>
        /// Adds a list of <see cref="DownloadTask"/>s to the download queue.
        /// </summary>
        /// <param name="downloadTasks">The list of <see cref="DownloadTask"/>s that will be checked and added.</param>
        /// <returns>Returns true if all downloadTasks were added successfully.</returns>
        public async Task<Result<bool>> AddToDownloadQueueAsync(List<DownloadTask> downloadTasks)
        {
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
                return Result.Fail(error).Add400BadRequestError();
            }

            Log.Debug($"Successfully added all {downloadTasks.Count} DownloadTasks");

            await CheckDownloadQueue();

            return Result.Ok(true);
        }

        private Result<PlexDownloadClient> GetDownloadClient(int downloadTaskId)
        {
            var downloadClient = DownloadsList.Find(x => x.DownloadTaskId == downloadTaskId);
            if (downloadClient == null)
            {
                return ResultExtensions.Create404NotFoundResult();
            }
            return Result.Ok(downloadClient);
        }

        /// <summary>
        /// Cancels the <see cref="PlexDownloadClient"/> executing the <see cref="DownloadTask"/> if it is downloading.
        /// Returns true if no client is executing the DownloadTask.
        /// </summary>
        /// <param name="downloadTaskId"></param>
        /// <returns></returns>
        public Result<bool> StopDownload(int downloadTaskId)
        {
            // Retrieve download client
            var downloadClient = GetDownloadClient(downloadTaskId);
            if (downloadClient.IsFailed)
            {
                return downloadClient.ToResult();
            }

            // TODO Check for status here first to give a better response back

            // Stop and cancel the download task
            var result = downloadClient.Value.Stop();
            if (result.IsFailed)
            {
                return result.WithError($"Failed to cancel downloadTask with id {downloadTaskId}").LogError();
            }
            return result;
        }


        private static bool SetMaxThreads()
        {
            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxConcurrentActiveRequests);

            // TODO This might have to be configurable from the front-end
            bool changeSucceeded = ThreadPool.SetMaxThreads(maxWorkerThreads, maxConcurrentActiveRequests);
            if (changeSucceeded)
            {
                Log.Information($"Successfully set the Max Threads for this platform to {maxWorkerThreads}");
            }
            else
            {
                Log.Error($"Could not set the Max Threads to {maxWorkerThreads}");
            }
            return changeSucceeded;
        }

        #endregion Methods
    }
}