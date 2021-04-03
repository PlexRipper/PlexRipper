using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.DTO.DownloadManager;
using PlexRipper.Application.PlexDownloads;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Common;

namespace PlexRipper.DownloadManager
{
    /// <summary>
    /// The DownloadQueue is responsible for deciding which downloadTask is handled by the <see cref="DownloadManager"/>.
    /// </summary>
    public class DownloadQueue : DownloadManagerBase, IDownloadQueue
    {
        private bool _isChecking;

        private Channel<IDownloadManager> _checkDownloadQueue = Channel.CreateBounded<IDownloadManager>(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadQueue"/> class.
        /// </summary>
        /// <param name="mediator">Defines a mediator to encapsulate request/response and publishing interaction patterns.</param>
        /// <param name="signalRService"></param>
        public DownloadQueue(IMediator mediator, ISignalRService signalRService) : base(mediator, signalRService) { }

        public async Task<Result> SetupAsync()
        {
            await ExecuteDownloadQueue();
            return Result.Ok();
        }

        public void CheckDownloadQueue(IDownloadManager downloadManager)
        {
            Task.Run(() => _checkDownloadQueue.Writer.WriteAsync(downloadManager));
        }

        public async Task ExecuteDownloadQueue()
        {
            Log.Information("Running DownloadQueue executor");

            await Task.Factory.StartNew(async () =>
            {
                while (await _checkDownloadQueue.Reader.WaitToReadAsync())
                {
                    var downloadManager = await _checkDownloadQueue.Reader.ReadAsync();
                    if (downloadManager == null)
                    {
                        Log.Error($"{nameof(DownloadManager)} was null.");
                        return;
                    }

                    Log.Debug("Checking for download tasks which can be processed.");
                    var serverListResult = await _mediator.Send(new GetAllDownloadTasksInPlexServersQuery(true));
                    var serverList = serverListResult.Value.Where(x => x.HasDownloadTasks).ToList();

                    Log.Information($"Starting the check of {serverList.Count} PlexServers.");
                    if (serverList.Any())
                    {
                        foreach (var plexServer in serverList)
                        {
                            var downloadTasks = plexServer.PlexLibraries.SelectMany(x => x.DownloadTasks).ToList();

                            // Set all initialized to Queued
                            foreach (var downloadTask in downloadTasks)
                            {
                                if (downloadTask.DownloadStatus == DownloadStatus.Initialized)
                                {
                                    downloadTask.DownloadStatus = DownloadStatus.Queued;
                                    await UpdateDownloadTaskStatusAsync(new DownloadClientUpdate(downloadTask));
                                }
                            }

                            // Retrieve next download task queued
                            var downloadingTask = downloadTasks.FirstOrDefault(x => x.DownloadStatus == DownloadStatus.Downloading);
                            if (downloadingTask != null)
                            {
                                Log.Warning($"PlexServer: {plexServer.Name} already has a download which is in currently downloading");
                                continue;
                            }

                            var queuedDownloadTask = downloadTasks.FirstOrDefault(x => x.DownloadStatus == DownloadStatus.Queued);
                            if (queuedDownloadTask != null)
                            {
                                Log.Debug(
                                    $"Returning the next Queued downloadTask with id {queuedDownloadTask.Id} - {queuedDownloadTask.Title} for server {plexServer.Name}");

                                await downloadManager.StartDownload(queuedDownloadTask.Id);
                            }

                            Log.Information($"There are no available downloadTasks remaining for PlexServer: {plexServer.Name}");
                        }
                    }
                    else
                    {
                        Log.Information("There are no PlexServers with DownloadTasks");
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}