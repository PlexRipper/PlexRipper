using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application;
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
        private readonly List<PlexDownloadClient> _downloadClientList = new();

        private readonly Dictionary<int, List<IDisposable>> _clientSubscriptions = new();

        private readonly CancellationTokenSource _tokenSource = new();

        private readonly Channel<int> _startQueue = Channel.CreateUnbounded<int>();

        #region Subjects

        private readonly Subject<DownloadTask> _downloadTaskStart = new();

        private readonly Subject<DownloadTask> _downloadTaskStopped = new();

        private readonly Subject<DownloadTask> _downloadTaskUpdate = new();

        private readonly Subject<DownloadTask> _downloadTaskFinished = new();

        #endregion

        private readonly IMediator _mediator;

        private readonly INotificationsService _notificationsService;

        private readonly Func<PlexDownloadClient> _plexDownloadClientFactory;

        private Task _executeDownloadTask;

        #endregion

        #region Constructor

        public DownloadTracker(IMediator mediator, INotificationsService notificationsService, Func<PlexDownloadClient> plexDownloadClientFactory)
        {
            _mediator = mediator;
            _notificationsService = notificationsService;
            _plexDownloadClientFactory = plexDownloadClientFactory;

            // Delete client once it has finished downloading
            DownloadTaskStopped.Subscribe(downloadTask => DeleteDownloadClient(downloadTask.Id));
            DownloadTaskFinished.Subscribe(downloadTask => DeleteDownloadClient(downloadTask.Id));
            DownloadTaskUpdate.DistinctUntilChanged(x => x.DownloadStatus).SubscribeAsync(OnDownloadStatusChanged);
        }

        #endregion

        #region Properties

        public IObservable<DownloadTask> DownloadTaskStart => _downloadTaskStart.AsObservable();

        public IObservable<DownloadTask> DownloadTaskStopped => _downloadTaskStopped.AsObservable();

        public IObservable<DownloadTask> DownloadTaskUpdate => _downloadTaskUpdate.AsObservable();

        /// <inheritdoc/>
        public IObservable<DownloadTask> DownloadTaskFinished => _downloadTaskFinished.AsObservable();

        public int ActiveDownloadClients => _downloadClientList.Count;

        public Task DownloadProcessTask => Task.WhenAll(_downloadClientList.Select(x => x.DownloadProcessTask ?? Task.CompletedTask));

        public bool IsBusy => !DownloadProcessTask.IsCompleted || _downloadClientList.Any();

        #endregion

        #region Public Methods

        public Result Setup()
        {
            Log.Information("Starting DownloadTrackerService");
            _executeDownloadTask = Task.Factory.StartNew(() => ExecuteDownloadTask(CancellationToken.None), TaskCreationOptions.LongRunning);
            return Result.Ok();
        }

        public async Task ExecuteDownloadTask(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var downloadTaskId = await _startQueue.Reader.ReadAsync(cancellationToken);
                var createResult = await CreateDownloadClientAsync(downloadTaskId, cancellationToken);
                if (createResult.IsFailed)
                    continue;

                createResult.Value.Start();
                _downloadTaskStart.OnNext(createResult.Value.DownloadTask);
            }
        }

        private async Task<Result<PlexDownloadClient>> CreateDownloadClientAsync(int downloadTaskId, CancellationToken cancellationToken)
        {
            var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId, true), cancellationToken);
            if (downloadTaskResult.IsFailed)
                return downloadTaskResult.ToResult().LogError();

            Log.Debug($"Creating Download client for {downloadTaskResult.Value.FullTitle}");

            // Create download client
            var newClient = await _plexDownloadClientFactory().Setup(downloadTaskResult.Value);
            if (newClient.IsFailed)
                return newClient.ToResult().LogError();

            SetupSubscriptions(newClient.Value);
            _downloadClientList.Add(newClient.Value);
            return Result.Ok(newClient.Value);
        }

        /// <summary>
        /// Deletes and disposes the <see cref="PlexDownloadClient"/> from the <see cref="DownloadManager"/>
        /// </summary>
        /// <param name="downloadTaskId">The <see cref="PlexDownloadClient"/> with this downloadTaskId</param>
        private void DeleteDownloadClient(int downloadTaskId)
        {
            Log.Debug($"Cleaning-up downloadClient with id {downloadTaskId}");
            var index = _downloadClientList.FindIndex(x => x.DownloadTaskId == downloadTaskId);
            if (index == -1)
            {
                Log.Debug($"Could not find downloadClient with downloadTaskId {downloadTaskId} to clean-up.");
                return;
            }

            if (_downloadClientList[index] is not null)
            {
                // Clean-up any subscriptions
                foreach (var disposable in _clientSubscriptions[downloadTaskId])
                {
                    disposable.Dispose();
                }

                _downloadClientList[index].Dispose();
                _downloadClientList.RemoveAt(index);
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
            var downloadClient = _downloadClientList.Find(x => x.DownloadTaskId == downloadTaskId);
            if (downloadClient is null)
                return ResultExtensions
                    .Create404NotFoundResult($"There is no DownloadClient currently working on a downloadTask with Id {downloadTaskId}");

            return Result.Ok(downloadClient);
        }

        #region Commands

        /// <inheritdoc/>
        public async Task<Result> StartDownloadClient(int downloadTaskId)
        {
            if (!_downloadClientList.Select(x => x.DownloadTask.Id).Contains(downloadTaskId))
            {
                await _startQueue.Writer.WriteAsync(downloadTaskId);
                return Result.Ok();
            }

            return Result.Fail($"The downloadTracker already has a DownloadClient with DownloadTask id {downloadTaskId}").LogError();
        }

        public async Task<Result> StopDownloadClient(int downloadTaskId)
        {
            if (downloadTaskId <= 0)
                return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            Log.Information($"Stopping DownloadClient for DownloadTaskId {downloadTaskId}");
            var downloadClient = GetDownloadClient(downloadTaskId);
            if (downloadClient.IsFailed)
                return downloadClient.ToResult();

            var stopResult = await downloadClient.Value.StopAsync();
            if (stopResult.IsFailed)
            {
                return stopResult;
            }

            _downloadTaskStopped.OnNext(downloadClient.Value.DownloadTask);
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

            _downloadTaskUpdate.OnNext(downloadClient.Value.DownloadTask);
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

        public Result<Task> GetDownloadProcessTask(int downloadTaskId)
        {
            var downloadClient = GetDownloadClient(downloadTaskId);
            if (downloadClient.IsFailed)
            {
                return downloadClient.ToResult();
            }

            return Result.Ok(downloadClient.Value.DownloadProcessTask);
        }

        #endregion

        #region Private Methods

        private void SetupSubscriptions(PlexDownloadClient newClient)
        {
            var subscriptions = new List<IDisposable>();

            subscriptions.Add(newClient
                .DownloadTaskUpdate
                .SubscribeAsync(OnDownloadTaskUpdate));

            // Download Worker Log subscription
            subscriptions.Add(newClient.DownloadWorkerLog
                .Buffer(TimeSpan.FromSeconds(1))
                .SubscribeAsync(logs => _mediator.Send(new AddDownloadWorkerLogsCommand(logs), _tokenSource.Token)));

            _clientSubscriptions.Add(newClient.DownloadTaskId, subscriptions);
        }

        private async Task OnDownloadTaskUpdate(DownloadTask downloadTask)
        {
            // Ensure the database has the latest update
            Log.Debug(downloadTask.ToString());
            var updateResult = await _mediator.Send(new UpdateDownloadTasksByIdCommand(new List<DownloadTask> { downloadTask }), _tokenSource.Token);
            if (updateResult.IsFailed)
            {
                updateResult.LogError();
            }

            _downloadTaskUpdate.OnNext(downloadTask);

            // Alert of a downloadTask that has finished
            if (downloadTask.DownloadStatus is DownloadStatus.DownloadFinished)
            {
                _downloadTaskFinished.OnNext(downloadTask);
            }
        }

        private async Task OnDownloadStatusChanged(DownloadTask downloadTask)
        {
            if (downloadTask.RootDownloadTaskId is not null)
            {
                // Update the download status of parent download tasks when a child changed
                var result = await _mediator.Send(new UpdateRootDownloadStatusOfDownloadTaskCommand(downloadTask.RootDownloadTaskId ?? 0),
                    _tokenSource.Token);
                if (result.IsFailed)
                {
                    result.LogError();
                }
            }
        }

        #endregion

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach ((_, List<IDisposable> value) in _clientSubscriptions)
            {
                foreach (var disposable in value)
                {
                    disposable.Dispose();
                }
            }

            foreach (var client in _downloadClientList)
            {
                if (client.DownloadStatus is DownloadStatus.Downloading)
                {
                    await client.PauseAsync();
                }

                client.Dispose();
            }
        }


    }
}