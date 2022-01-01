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

namespace PlexRipper.DownloadManager
{
    /// <summary>
    /// The DownloadQueue is responsible for deciding which downloadTask is handled by the <see cref="DownloadManager"/>.
    /// </summary>
    public class DownloadQueue : IDownloadQueue
    {
        #region Fields

        private readonly IMediator _mediator;

        private readonly IDownloadTaskValidator _downloadTaskValidator;

        private readonly Channel<int> _plexServersToCheckChannel = Channel.CreateUnbounded<int>();

        private readonly Subject<int> _serverCompletedDownloading = new();

        private readonly Subject<DownloadTask> _startDownloadTask = new();

        private readonly CancellationToken _token = new();

        private readonly Subject<List<DownloadTask>> _updateDownloadTasks = new();

        private Task<Task> _copyTask;

        #endregion

        #region Constructor

        public DownloadQueue(IMediator mediator, IDownloadTaskValidator downloadTaskValidator)
        {
            _mediator = mediator;
            _downloadTaskValidator = downloadTaskValidator;
        }

        #endregion

        #region Properties

        public bool IsBusy => _plexServersToCheckChannel.Reader.Count > 0;

        /// <summary>
        /// Emits the id of a <see cref="PlexServer"/> which has no more <see cref="DownloadTask">DownloadTasks</see> to process.
        /// </summary>
        public IObservable<int> ServerCompletedDownloading => _serverCompletedDownloading.AsObservable();

        public IObservable<DownloadTask> StartDownloadTask => _startDownloadTask.AsObservable();

        #endregion

        #region Public Methods

        public Result Setup()
        {
            _copyTask = Task.Factory.StartNew(ExecuteDownloadQueueCheck, TaskCreationOptions.LongRunning);
            return _copyTask.IsFaulted ? Result.Fail("ExecuteFileTasks failed due to an error").LogError() : Result.Ok();
        }

        /// <inheritdoc/>
        public async Task<Result> AddToDownloadQueueAsync(List<DownloadTask> downloadTasks)
        {
            if (!downloadTasks.Any())
                return ResultExtensions.IsEmpty(nameof(downloadTasks)).LogWarning();

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

            Log.Debug($"Successfully added all {validateResult.Value.Count} DownloadTasks");
            var uniquePlexServers = downloadTasks.Select(x => x.PlexServerId).Distinct().ToList();
            await CheckDownloadQueue(uniquePlexServers);
            return Result.Ok();
        }

        /// <summary>
        ///  Determines the next downloadable <see cref="DownloadTask"/>.
        /// Will only return a successful result if a queued task can be found
        /// </summary>
        /// <param name="downloadTasks"></param>
        /// <returns></returns>
        public static Result<DownloadTask> GetNextDownloadTask(ref List<DownloadTask> downloadTasks)
        {
            // Check if there is anything downloading already
            var nextDownloadTask = downloadTasks.FirstOrDefault(x => x.DownloadStatus == DownloadStatus.Downloading);
            if (nextDownloadTask is not null)
            {
                // Should we check deeper for any nested queued tasks inside downloading tasks
                if (nextDownloadTask.Children is not null && nextDownloadTask.Children.Any())
                {
                    var children = nextDownloadTask.Children;
                    return GetNextDownloadTask(ref children);
                }

                return Result.Fail($"DownloadTask {nextDownloadTask.Title} is already downloading").LogDebug();
            }

            nextDownloadTask = downloadTasks.FirstOrDefault(x => x.DownloadStatus == DownloadStatus.Queued);
            if (nextDownloadTask is null)
                return Result.Fail("There were no downloadTasks left to download.").LogDebug();

            // Should we check deeper for any nested queued tasks in downloading tasks
            if (nextDownloadTask.Children is not null && nextDownloadTask.Children.Any())
            {
                var children = nextDownloadTask.Children;
                return GetNextDownloadTask(ref children);
            }

            return Result.Ok(nextDownloadTask);
        }

        /// <summary>
        /// Check the DownloadQueue for downloadTasks which can be started.
        /// </summary>
        public async Task<Result> CheckDownloadQueue(List<int> plexServerIds)
        {
            if (!plexServerIds.Any())
                return ResultExtensions.IsEmpty(nameof(plexServerIds)).LogWarning();

            Log.Information($"Adding {plexServerIds.Count} {nameof(PlexServer)}s to the DownloadQueue to check for the next download.");
            foreach (var plexServerId in plexServerIds)
            {
                await _plexServersToCheckChannel.Writer.WriteAsync(plexServerId);
            }

            return Result.Ok();
        }

        public async Task<Result> CheckDownloadQueueServer(int plexServerId)
        {
            if (plexServerId <= 0)
                return ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId).LogWarning();

            var downloadTasksResult = await _mediator.Send(new GetDownloadTasksByPlexServerIdQuery(plexServerId));
            if (downloadTasksResult.IsFailed)
                return downloadTasksResult.LogError();

            var plexServerName = await _mediator.Send(new GetPlexServerNameByIdQuery(plexServerId));
            if (plexServerName.IsFailed)
                return plexServerName.LogError();

            Log.Debug($"Checking {nameof(PlexServer)}: {plexServerName.Value} for the next download to start");

            var downloadTasks = downloadTasksResult.Value;

            var nextDownloadTask = GetNextDownloadTask(ref downloadTasks);
            if (nextDownloadTask.IsFailed)
            {
                Log.Information($"There are no available downloadTasks remaining for PlexServer with Id: {plexServerName.Value}");
                _serverCompletedDownloading.OnNext(plexServerId);
                return Result.Ok();
            }

            Log.Information($"Selected download task {nextDownloadTask.Value.FullTitle} to start as the next task");

            _startDownloadTask.OnNext(nextDownloadTask.Value);

            return Result.Ok();
        }

        #endregion

        #region Private Methods

        private async Task ExecuteDownloadQueueCheck()
        {
            while (!_token.IsCancellationRequested)
            {
                var item = await _plexServersToCheckChannel.Reader.ReadAsync();
                await CheckDownloadQueueServer(item);
            }
        }

        #endregion
    }
}