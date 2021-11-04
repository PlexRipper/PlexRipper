using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Application.Common;
using PlexRipper.Application.DownloadWorkerTasks;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager.Download
{
    /// <summary>
    /// The PlexDownloadClient handles a single <see cref="DownloadTask"/> at a time and
    /// manages the <see cref="DownloadWorker"/>s responsible for the multi-threaded downloading.
    /// </summary>
    public class PlexDownloadClient : IDisposable
    {
        private readonly Func<DownloadWorkerTask, DownloadWorker> _downloadWorkerFactory;

        private readonly IUserSettings _userSettings;

        private readonly IMediator _mediator;

        #region Fields

        private readonly List<DownloadWorker> _downloadWorkers = new();

        private readonly EventLoopScheduler _timeThreadContext = new();

        private readonly Subject<DownloadTask> _downloadTaskUpdate = new();

        private readonly Subject<DownloadWorkerLog> _downloadWorkerLog = new();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlexDownloadClient"/> class.
        /// </summary>
        /// <param name="downloadWorkerFactory"></param>
        /// <param name="userSettings"></param>
        /// <param name="mediator"></param>
        public PlexDownloadClient(
            Func<DownloadWorkerTask, DownloadWorker> downloadWorkerFactory,
            IUserSettings userSettings,
            IMediator mediator)
        {
            _downloadWorkerFactory = downloadWorkerFactory;
            _userSettings = userSettings;
            _mediator = mediator;
        }

        /// <summary>
        /// Setup this <see cref="PlexDownloadClient"/> to start execute work.
        /// This needs to be called before any other action can be taken.
        /// </summary>
        /// <param name="downloadTask">The <see cref="DownloadTask"/> to start executing.</param>
        /// <returns></returns>
        public async Task<Result<PlexDownloadClient>> Setup(DownloadTask downloadTask)
        {
            if (downloadTask is null)
            {
                return ResultExtensions.IsNull(nameof(downloadTask));
            }

            DownloadTask = downloadTask;

            if (DownloadTask.PlexServer is null)
            {
                return ResultExtensions.IsNull($"{nameof(DownloadTask)}.{nameof(DownloadTask.PlexServer)}");
            }

            if (!DownloadTask.DownloadWorkerTasks.Any())
            {
                var downloadWorkerTasks = await GenerateDownloadWorkerTasks(DownloadTask, _userSettings.DownloadSegments);
                if (downloadWorkerTasks.IsFailed)
                {
                    return downloadWorkerTasks.ToResult();
                }

                DownloadTask.DownloadWorkerTasks = downloadWorkerTasks.Value;
            }

            var createResult = await CreateDownloadWorkers(downloadTask);
            if (createResult.IsFailed)
            {
                return createResult.ToResult();
            }

            // TODO Re-enable when implementing downloadSpeedLimit
            //SetDownloadSpeedLimit(_userSettings.GetDownloadSpeedLimit(DownloadTask.PlexServer.MachineIdentifier));

            SetupSubscriptions();

            return Result.Ok(this);
        }

        private async Task<Result<List<DownloadWorkerTask>>> GenerateDownloadWorkerTasks(DownloadTask downloadTask, int parts)
        {
            if (parts <= 0)
                return Result.Fail($"Parameter {nameof(parts)} was {parts}, prevented division by invalid value").LogWarning();

            // Create download worker tasks/segments/ranges
            var totalBytesToReceive = downloadTask.DataTotal;
            var partSize = totalBytesToReceive / parts;
            var remainder = totalBytesToReceive - partSize * parts;

            var downloadWorkerTasks = new List<DownloadWorkerTask>();

            for (var i = 0; i < parts; i++)
            {
                var start = partSize * i;
                var end = start + partSize;
                if (i == parts - 1 && remainder > 0)
                {
                    // Add the remainder to the last download range
                    end += remainder;
                }

                downloadWorkerTasks.Add(new DownloadWorkerTask(downloadTask, i + 1, start, end));
            }

            var addResult = await _mediator.Send(new AddDownloadWorkerTasksCommand(downloadTask.DownloadWorkerTasks));
            if (addResult.IsFailed)
            {
                return addResult.ToResult();
            }

            return Result.Ok(downloadWorkerTasks);
        }

        private async Task<Result<List<DownloadWorkerTask>>> CreateDownloadWorkers(DownloadTask downloadTask)
        {
            if (downloadTask is null)
                return ResultExtensions.IsNull(nameof(downloadTask)).LogWarning();

            if (!downloadTask.DownloadWorkerTasks.Any())
                return ResultExtensions.IsEmpty($"{nameof(downloadTask)}.{nameof(downloadTask.DownloadWorkerTasks)}").LogWarning();

            var downloadWorkerTasksResult = await _mediator.Send(new GetAllDownloadWorkerTasksByDownloadTaskIdQuery(downloadTask.Id));
            if (downloadWorkerTasksResult.IsFailed)
            {
                return downloadWorkerTasksResult.ToResult();
            }

            foreach (var downloadWorkerTask in downloadWorkerTasksResult.Value)
            {
                _downloadWorkers.Add(_downloadWorkerFactory(downloadWorkerTask));
            }

            return Result.Ok();
        }

        #endregion

        #region Properties

        public DownloadStatus DownloadStatus
        {
            get => DownloadTask.DownloadStatus;
            internal set => DownloadTask.DownloadStatus = value;
        }

        public DownloadTask DownloadTask { get; internal set; }

        /// <summary>
        /// The ClientId/DownloadTaskId is always the same id that is assigned to the <see cref="DownloadTask"/>.
        /// </summary>
        public int DownloadTaskId => DownloadTask.Id;

        /// <summary>
        /// Gets the Task that completes when all download workers have finished.
        /// </summary>
        public Task DownloadProcessTask => Task.WhenAll(_downloadWorkers.Select(x => x.DownloadProcessTask));

        #region Observables

        public IObservable<DownloadTask> DownloadTaskUpdate => _downloadTaskUpdate.AsObservable();

        public IObservable<DownloadWorkerLog> DownloadWorkerLog => _downloadWorkerLog.AsObservable();

        #endregion

        #endregion

        #region Methods

        #region Private

        /// <summary>
        /// Releases the unmanaged resources used by the HttpClient and optionally disposes of the managed resources.
        /// </summary>
        public void Dispose()
        {
            ClearDownloadWorkers();
        }

        private void SetupSubscriptions()
        {
            if (!_downloadWorkers.Any())
            {
                Log.Warning("No download workers have been made yet, cannot setup subscriptions.");
                return;
            }

            // On download worker update
            _downloadWorkers
                .Select(x => x.DownloadWorkerTaskUpdate)
                .CombineLatest()
                .Sample(TimeSpan.FromMilliseconds(1000), _timeThreadContext)
                .Subscribe(OnDownloadWorkerTaskUpdate);

            // On download worker log
            _downloadWorkers
                .Select(x => x.DownloadWorkerLog)
                .Merge()
                .Subscribe(downloadWorkerLog => _downloadWorkerLog.OnNext(downloadWorkerLog));
        }

        /// <summary>
        /// Calls Dispose on all DownloadWorkers and clears the downloadWorkersList.
        /// </summary>
        /// <returns>Is successful.</returns>
        private void ClearDownloadWorkers()
        {
            if (_downloadWorkers.Any())
            {
                _downloadWorkers.ForEach(x => x.Dispose());
                _downloadWorkers.Clear();
            }

            if (DownloadTask is not null)
            {
                Log.Debug($"DownloadWorkers have been disposed for {DownloadTask.DownloadDirectory}");
            }
        }

        #region Subscriptions

        private void OnDownloadWorkerTaskUpdate(IList<DownloadWorkerTask> downloadWorkerUpdates)
        {
            if (_downloadTaskUpdate.IsDisposed)
            {
                return;
            }

            // Replace every DownloadWorkerTask with the updated version
            foreach (var downloadWorkerTask in downloadWorkerUpdates)
            {
                var i = DownloadTask.DownloadWorkerTasks.FindIndex(x => x.Id == downloadWorkerTask.Id);
                if (i > -1)
                {
                    DownloadTask.DownloadWorkerTasks[i] = downloadWorkerTask;
                }
            }

            var clientStatus = downloadWorkerUpdates.Select(x => x.DownloadStatus).ToList();

            // If there is any error then set client to error state
            if (clientStatus.Any(x => x == DownloadStatus.Error))
            {
                DownloadStatus = DownloadStatus.Error;
            }

            if (clientStatus.Any(x => x == DownloadStatus.Downloading))
            {
                DownloadStatus = DownloadStatus.Downloading;
            }

            if (clientStatus.All(x => x == DownloadStatus.Completed))
            {
                DownloadStatus = DownloadStatus.Completed;
            }

            _downloadTaskUpdate.OnNext(DownloadTask);

            if (DownloadStatus == DownloadStatus.Completed)
            {
                _downloadTaskUpdate.OnCompleted();
                _downloadTaskUpdate.OnCompleted();
            }
        }

        #endregion

        #endregion

        #region Public

        #region Commands

        /// <summary>
        /// Starts the download workers for the <see cref="DownloadTask"/> given during setup.
        /// </summary>
        /// <returns>Is successful.</returns>
        public Result Start()
        {
            if (DownloadStatus == DownloadStatus.Downloading)
            {
                return Result.Fail("The PlexDownloadClient is already downloading and can not be started.");
            }

            Log.Debug($"Start downloading {DownloadTask.FileName}");
            try
            {
                foreach (var downloadWorker in _downloadWorkers)
                {
                    var startResult = downloadWorker.Start();
                    if (startResult.IsFailed)
                    {
                        return startResult.LogError();
                    }
                }
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)
                        .WithMessage($"Could not download {DownloadTask.FileName}"))
                    .LogError();
            }

            return Result.Ok();
        }

        public async Task<Result<DownloadTask>> PauseAsync()
        {
            if (DownloadStatus != DownloadStatus.Downloading)
            {
                Log.Warning($"DownloadClient with {DownloadTask.FileName} is currently not downloading and cannot be paused.");
            }

            Log.Information($"Pause downloading of {DownloadTask.FileName}");

            await Task.WhenAll(_downloadWorkers.Select(x => x.PauseAsync()));

            DownloadTask.DownloadWorkerTasks = _downloadWorkers.Select(x => x.DownloadWorkerTask).ToList();
            return Result.Ok(DownloadTask);
        }

        public async Task<Result<DownloadTask>> StopAsync()
        {
            Log.Information($"Stop downloading {DownloadTask.FileName} from {DownloadTask.DownloadUrl}");

            await Task.WhenAll(_downloadWorkers.Select(x => x.StopAsync()));

            DownloadTask.DownloadWorkerTasks = _downloadWorkers.Select(x => x.DownloadWorkerTask).ToList();
            return Result.Ok(DownloadTask);
        }

        public void SetDownloadSpeedLimit(int downloadSpeedLimitInKb = 0)
        {
            _downloadWorkers.ForEach(x => x.SetDownloadSpeedLimit(downloadSpeedLimitInKb / _downloadWorkers.Count));
        }

        #endregion

        #endregion

        #endregion
    }
}