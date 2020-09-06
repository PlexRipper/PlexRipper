using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.PlexDownloads.Commands;
using PlexRipper.Application.PlexDownloads.Queries;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Download;

namespace PlexRipper.DownloadManager
{
    /// <summary>
    /// The DownloadQueue is responsible for deciding which downloadTask is handled by the <see cref="DownloadManager"/>
    /// </summary>
    public class DownloadQueue
    {
        private readonly IMediator _mediator;
        private bool _isChecking = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadQueue"/> class.
        /// </summary>
        /// <param name="mediator">Defines a mediator to encapsulate request/response and publishing interaction patterns.</param>
        public DownloadQueue(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<bool>> UpdateDownloadQueue()
        {
            if (_isChecking)
            {
                return Result.Fail("Update download Queue already in progress").LogWarning();
            }

            _isChecking = true;

            Log.Debug("Checking for download tasks which can be processed.");
            var serverList = await _mediator.Send(new GetAllDownloadTasksInPlexServersQuery(true, true));

            if (!serverList.Value.Any())
            {
                Log.Debug("No download tasks found to start processing.");

                return Result.Ok(true);
            }

            Log.Information($"Starting the check of {serverList.Value.Count} downloadTasks.");

            foreach (var server in serverList.Value)
            {
                PlexDownloadClient currentDownload = null;
                var downloadTasks = server.PlexLibraries.SelectMany(x => x.DownloadTasks).ToList();

                // Set all initialized to Queued
                downloadTasks.FindAll(x => x.DownloadStatus == DownloadStatus.Initialized).ForEach(async x =>
                {
                    x.DownloadStatus = DownloadStatus.Queued;
                    await _mediator.Send(new UpdateDownloadStatusOfDownloadTaskCommand(x.Id, DownloadStatus.Queued));
                });
                //
                // if (downloadTasks.FindAll(x => x.DownloadStatus == DownloadStatus.Downloading).Count == 0)
                // {
                //     downloadTasks.FirstOrDefault(x => x.DownloadStatus == DownloadStatus.Queued)
                // }
            }

            _isChecking = false;
            return Result.Ok(true);
        }

        public async Task<Result<DownloadTask>> NextDownloadAsync(PlexServer plexServer)
        {
            if (plexServer == null)
            {
                return ResultExtensions.IsNull(nameof(plexServer)).LogError();
            }

            await UpdateDownloadQueue();

            var serverResult = await _mediator.Send(new GetDownloadTasksByPlexServerIdQuery(plexServer.Id, true, true));
            if (serverResult.IsFailed)
            {
                return serverResult.ToResult();
            }

            var downloadTasks = serverResult.Value.PlexLibraries.SelectMany(x => x.DownloadTasks).ToList();
            var downloadingTask = downloadTasks.FirstOrDefault(x => x.DownloadStatus == DownloadStatus.Downloading);
            if (downloadingTask != null)
            {
                Log.Warning($"PlexServer {plexServer.Name} already has a download which is in currently downloading, returning that downloadTask.");

                return Result.Ok(downloadingTask);
            }

            var queuedDownloadTask = downloadTasks.FirstOrDefault(x => x.DownloadStatus == DownloadStatus.Queued);
            if (queuedDownloadTask != null)
            {
                Log.Debug(
                    $"Returning the next Queued downloadTask with id {queuedDownloadTask.Id} - {queuedDownloadTask.Title} for server {plexServer.Name}");

                return Result.Ok(queuedDownloadTask);
            }

            return Result.Fail($"There are no available downloadTasks remaining for PlexServer: {plexServer.Name}");
        }
    }
}