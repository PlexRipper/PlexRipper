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
using System.Threading.Tasks;
using PlexRipper.Domain.Common;

namespace PlexRipper.DownloadManager
{
    public class DownloadManager : IDownloadManager
    {
        #region Fields

        private readonly IMediator _mediator;
        private readonly IHubContext<DownloadHub> _hubContext;
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

        public DownloadManager(IMediator mediator, IHubContext<DownloadHub> hubContext, IUserSettings userSettings, IFileSystem fileSystem)
        {
            _mediator = mediator;
            _hubContext = hubContext;
            _userSettings = userSettings;
            _fileSystem = fileSystem;
        }

        #endregion Constructors

        #region Methods

        private PlexDownloadClient CreateDownloadClient(DownloadTask downloadTask)
        {
            PlexDownloadClient newClient = new PlexDownloadClient(downloadTask, _fileSystem);

            newClient.DownloadProgressChanged += OnDownloadProgressChanged;

            //newClient.DownloadFileCompleted += OnDownloadFileCompleted;

            DownloadsList.Add(newClient);
            return newClient;
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Log.Information("The download has completed!");
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgress downloadProgress)
        {
            var plexDownloadClient = sender as PlexDownloadClient;

            _hubContext.Clients.All.SendAsync("DownloadProgress", downloadProgress);

            Log.Information(
                $"({plexDownloadClient.ClientId}){plexDownloadClient.DownloadTask.FileName} => Downloaded {DataFormat.FormatSizeString(downloadProgress.DataReceived)} of {DataFormat.FormatSizeString(downloadProgress.DataTotal)} bytes ({downloadProgress.DownloadSpeed}). {downloadProgress.Percentage} % complete...");
        }

        public async Task<Result> StartDownloadAsync(DownloadTask downloadTask)
        {
            // Add to DB
            Log.Debug($"Adding new downloadTask: {downloadTask.Title}");
            var result = await _mediator.Send(new AddDownloadTaskCommand(downloadTask));
            if (result.IsFailed)
            {
                return result;
            }
            var downloadTaskDB = await _mediator.Send(new GetDownloadTaskByIdQuery(result.Value.Id));
            if (downloadTaskDB.IsFailed)
            {
                return result;
            }

            try
            {
                // TODO This might be removed if the authToken is stored in the database.
                downloadTaskDB.Value.PlexServerAuthToken = downloadTask.PlexServerAuthToken;

                Log.Debug(downloadTaskDB.Value.ToString());
                var downloadClient = CreateDownloadClient(downloadTaskDB.Value);
                return await downloadClient.StartAsync();
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to start the Download of {downloadTask.FileName}");
                throw;
            }
        }

        public Task<Result> StartDownloadAsync(List<DownloadTask> downloadTasks)
        {
            Log.Error("Not Implemented");
            return Task.Run(Result.Ok);
        }

        private Result<PlexDownloadClient> GetDownloadClient(int downloadTaskId)
        {
            var downloadClient = DownloadsList.Find(x => x.ClientId == downloadTaskId);
            if (downloadClient == null)
            {
                return ResultExtensions.Get404NotFoundResult();
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

        #endregion Methods
    }
}