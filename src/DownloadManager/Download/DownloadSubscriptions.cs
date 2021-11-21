using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.Domain.RxNet;

namespace PlexRipper.DownloadManager
{
    public class DownloadSubscriptions : IDownloadSubscriptions
    {
        private readonly IMediator _mediator;

        private readonly IFileMerger _fileMerger;

        private readonly IDownloadTracker _downloadTracker;

        private readonly IDownloadQueue _downloadQueue;

        private readonly IDownloadCommands _downloadCommands;

        private readonly IDownloadProgressScheduler _downloadProgressScheduler;

        public DownloadSubscriptions(
            IMediator mediator,
            IFileMerger fileMerger,
            IDownloadTracker downloadTracker,
            IDownloadQueue downloadQueue,
            IDownloadCommands downloadCommands,
            IDownloadProgressScheduler downloadProgressScheduler)
        {
            _mediator = mediator;
            _fileMerger = fileMerger;
            _downloadTracker = downloadTracker;
            _downloadQueue = downloadQueue;
            _downloadCommands = downloadCommands;
            _downloadProgressScheduler = downloadProgressScheduler;
        }

        public Result Setup()
        {
            // Start DownloadTask when given by the DownloadQueue
            _downloadQueue
                .StartDownloadTask
                .SubscribeAsync(async downloadTask => await _downloadCommands.StartDownloadTaskAsync(downloadTask));

            // Update database on downloadTask updates
            _downloadTracker
                .DownloadTaskUpdate
                .SubscribeAsync(UpdateDownloadTaskAsync);

            // Start sending updates to the front-end when starting a DownloadTask
            _downloadTracker
                .DownloadTaskStart
                .SubscribeAsync(async downloadTask =>
                    await _downloadProgressScheduler.StartDownloadProgressJob(downloadTask.PlexServerId));

            // Halt progress updates for PlexServers that are no more active
            _downloadQueue
                .ServerCompletedDownloading
                .SubscribeAsync(async plexServerId => await _downloadProgressScheduler.StopDownloadProgressJob(plexServerId));

            // Start merging or moving file once the download has finished
            _downloadTracker
                .DownloadTaskFinished
                .SubscribeAsync(OnDownloadFileCompleted);

            _fileMerger
                .FileMergeProgressObservable
                .SubscribeAsync(OnFileMergeProgress);

            return Result.Ok();
        }

        private async Task UpdateDownloadTaskAsync(DownloadTask downloadTask)
        {
            Log.Debug(downloadTask.ToString());
            var updateResult = await _mediator.Send(new UpdateDownloadTasksByIdCommand(new List<DownloadTask> { downloadTask }));
            if (updateResult.IsFailed)
            {
                updateResult.LogError();
            }
        }

        private async Task OnFileMergeProgress(FileMergeProgress progress)
        {
            Log.Debug(
                $"Merge Progress: {progress.DataTransferred} / {progress.DataTotal} - {progress.Percentage} - {progress.TransferSpeedFormatted}");
            if (progress.Percentage >= 100)
            {
                var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(progress.DownloadTaskId));
                if (downloadTaskResult.IsFailed)
                {
                    downloadTaskResult.LogError();
                    return;
                }

                downloadTaskResult.Value.DownloadStatus = DownloadStatus.Completed;
                await UpdateDownloadTaskAsync(downloadTaskResult.Value);
            }
        }

        private async Task OnDownloadFileCompleted(DownloadTask downloadTask)
        {
            if (downloadTask.MediaParts == 1)
            {
                downloadTask.DownloadStatus = DownloadStatus.Moving;
                downloadTask.DownloadWorkerTasks.ForEach(x => x.DownloadStatus = DownloadStatus.Moving);
            }

            if (downloadTask.MediaParts > 1)
            {
                downloadTask.DownloadStatus = DownloadStatus.Merging;
                downloadTask.DownloadWorkerTasks.ForEach(x => x.DownloadStatus = DownloadStatus.Merging);
            }

            await UpdateDownloadTaskAsync(downloadTask);
            var addFileTaskResult = await _fileMerger.AddFileTaskFromDownloadTask(downloadTask.Id);
            if (addFileTaskResult.IsFailed)
            {
                addFileTaskResult.LogError();
            }

            Log.Information($"The download of {downloadTask.Title} has finished!");
            await _downloadQueue.CheckDownloadQueue();
        }
    }
}