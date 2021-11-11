﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.Domain.RxNet;
using PlexRipper.DownloadManager.DownloadClient;

namespace PlexRipper.DownloadManager
{
    public class DownloadTracker : IDownloadTracker
    {
        #region Fields

        /// <summary>
        /// Currently loaded and active <see cref="PlexDownloadClient"/>s.
        /// </summary>
        private readonly List<PlexDownloadClient> _downloadsList = new();

        #region Subjects

        private readonly Subject<DownloadTask> _downloadTaskStart = new();

        private readonly Subject<DownloadTask> _downloadTaskUpdate = new();

        private readonly Subject<DownloadTask> _downloadTaskCompleted = new();

        #endregion

        private readonly IMediator _mediator;

        private readonly INotificationsService _notificationsService;

        private readonly Func<PlexDownloadClient> _plexDownloadClientFactory;

        #endregion

        #region Constructor

        public DownloadTracker(IMediator mediator, INotificationsService notificationsService, Func<PlexDownloadClient> plexDownloadClientFactory)
        {
            _mediator = mediator;
            _notificationsService = notificationsService;
            _plexDownloadClientFactory = plexDownloadClientFactory;
        }

        #endregion

        #region Properties

        public IObservable<DownloadTask> DownloadTaskStart => _downloadTaskStart.AsObservable();

        public IObservable<DownloadTask> DownloadTaskUpdate => _downloadTaskUpdate.AsObservable();

        public IObservable<DownloadTask> DownloadTaskCompleted => _downloadTaskCompleted.AsObservable();

        public int ActiveDownloadClients => _downloadsList.Count;

        #endregion

        #region Public Methods

        private async Task<Result<PlexDownloadClient>> CreateDownloadClient(int downloadTaskId)
        {
            var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId));
            if (downloadTaskResult.IsFailed)
            {
                return downloadTaskResult.ToResult();
            }

            return await CreateDownloadClient(downloadTaskResult.Value);
        }

        public async Task<Result<PlexDownloadClient>> CreateDownloadClient(DownloadTask downloadTask)
        {
            if (downloadTask is null)
                return ResultExtensions.IsNull(nameof(downloadTask)).LogWarning();

            if (!downloadTask.IsDownloadable)
                return Result.Fail($"DownloadTask {downloadTask.FullTitle} is not downloadable").LogWarning();

            Log.Debug($"Creating Download client for {downloadTask.FullTitle}");

            // Create download client
            var newClient = await _plexDownloadClientFactory().Setup(downloadTask);
            if (newClient.IsFailed)
                return newClient.ToResult().LogError();

            SetupSubscriptions(newClient.Value);
            _downloadsList.Add(newClient.Value);
            return Result.Ok(newClient.Value);
        }

        /// <summary>
        /// Deletes and disposes the <see cref="PlexDownloadClient"/> from the <see cref="DownloadManager"/>
        /// </summary>
        /// <param name="downloadTaskId">The <see cref="PlexDownloadClient"/> with this downloadTaskId</param>
        private void DeleteDownloadClient(int downloadTaskId)
        {
            Log.Debug($"Cleaning-up downloadClient with id {downloadTaskId}");
            var index = _downloadsList.FindIndex(x => x.DownloadTaskId == downloadTaskId);
            if (index == -1)
            {
                Log.Debug($"Could not find downloadClient with downloadTaskId {downloadTaskId} to clean-up.");
                return;
            }

            if (_downloadsList[index] is not null)
            {
                _downloadsList[index].Dispose();
                _downloadsList.RemoveAt(index);
            }

            Log.Debug($"Cleaned-up PlexDownloadClient with id {downloadTaskId} from the DownloadManager");
        }

        /// <summary>
        /// Check if a <see cref="PlexDownloadClient"/> has already been assigned to this <see cref="DownloadTask"/>.
        /// </summary>
        /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/>.</param>
        /// <returns>Returns the <see cref="PlexDownloadClient"/> if found and fails otherwise.</returns>
        private Result<PlexDownloadClient> GetDownloadClient(int downloadTaskId)
        {
            var downloadClient = _downloadsList.Find(x => x.DownloadTaskId == downloadTaskId);
            if (downloadClient is null)
                return ResultExtensions
                    .Create404NotFoundResult($"There is no DownloadClient currently working on a downloadTask with Id {downloadTaskId}");

            return Result.Ok(downloadClient);
        }

        #region Commands

        /// <summary>
        /// Will find, or create, a <see cref="PlexDownloadClient"/> and start the download process.
        /// Note: This <see cref="Task"/> only completed once the download process is done
        /// </summary>
        /// <param name="downloadTaskId"></param>
        /// <returns></returns>
        public async Task<Result> StartDownloadClient(int downloadTaskId)
        {
            var downloadClient = GetDownloadClient(downloadTaskId);
            if (downloadClient.IsFailed)
            {
                downloadClient = await CreateDownloadClient(downloadTaskId);
                if (downloadClient.IsFailed)
                {
                    await _notificationsService.SendResult(downloadClient);
                    return downloadClient.ToResult().LogError();
                }
            }

            downloadClient.Value.Start();
            await downloadClient.Value.DownloadProcessTask;
            return Result.Ok();
        }

        public async Task<Result> StopDownloadClient(int downloadTaskId)
        {
            if (downloadTaskId <= 0)
                return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            var downloadClient = GetDownloadClient(downloadTaskId);
            if (downloadClient.IsFailed)
                return downloadClient.ToResult();

            var stopResult = await downloadClient.Value.StopAsync();
            if (stopResult.IsFailed)
            {
                return stopResult;
            }

            DeleteDownloadClient(downloadTaskId);

            return Result.Ok();
        }

        public async Task<Result> PauseDownloadClient(int downloadTaskId)
        {
            if (downloadTaskId <= 0)
                return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            var downloadClient = GetDownloadClient(downloadTaskId);
            if (downloadClient.IsFailed)
                return downloadClient.ToResult();

            var pauseResult = await downloadClient.Value.PauseAsync();
            if (pauseResult.IsFailed)
            {
                return pauseResult;
            }

            DeleteDownloadClient(downloadTaskId);

            return Result.Ok();
        }

        #endregion

        public bool IsDownloading(int downloadTaskId)
        {
            var downloadClient = GetDownloadClient(downloadTaskId);
            if (downloadClient.IsFailed)
            {
                return false;
            }

            return downloadClient.Value.DownloadStatus is DownloadStatus.Downloading;
        }

        #endregion

        #region Private Methods

        private void SetupSubscriptions(PlexDownloadClient newClient)
        {
            newClient
                .DownloadTaskUpdate
                .Subscribe(value => _downloadTaskUpdate.OnNext(value));

            // TODO make seperate completed for PlexDownloadClient which fires after everything has been cleaned-up
            newClient
                .DownloadTaskUpdate
                .Where(x => x.DownloadStatus is DownloadStatus.Completed)
                .Subscribe(value => _downloadTaskCompleted.OnNext(value));

            // Download Worker Log subscription
            newClient.DownloadWorkerLog
                .Buffer(TimeSpan.FromSeconds(1))
                .SubscribeAsync(logs => _mediator.Send(new AddDownloadWorkerLogsCommand(logs)));
        }

        #endregion
    }
}