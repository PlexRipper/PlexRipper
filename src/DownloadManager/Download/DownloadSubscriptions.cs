using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application;
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

            // Start sending updates to the front-end when starting a DownloadTask
            _downloadTracker
                .DownloadTaskStart
                .SubscribeAsync(async downloadTask =>
                    await _downloadProgressScheduler.StartDownloadProgressJob(downloadTask.PlexServerId));

            // Start merging or moving file once the download has finished
            _downloadTracker
                .DownloadTaskFinished
                .SubscribeAsync(OnDownloadFileCompleted);

            // On big events send and extra update to front-end to minimize the delay
            Observable.Merge(new[]
            {
                _downloadTracker.DownloadTaskStart,
                _downloadTracker.DownloadTaskFinished,
            }).SubscribeAsync(downloadTask => _downloadProgressScheduler.FireDownloadProgressJob(downloadTask.PlexServerId));

            _fileMerger
                .FileMergeCompletedObservable
                .SubscribeAsync(OnFileMergeCompleted);

            _fileMerger
                .FileMergeStartObservable
                .SubscribeAsync(task => _downloadProgressScheduler.FireDownloadProgressJob(task.DownloadTask.PlexServerId));

            _fileMerger
                .FileMergeProgressObservable
                .SubscribeAsync(OnFileMergeProgress);
            return Result.Ok();
        }

        private async Task OnFileMergeCompleted(FileMergeProgress task)
        {
            Log.Debug("FileTask has completed");

            var rootDownloadTaskIdResult = await _mediator.Send(new GetRootDownloadTaskIdByDownloadTaskIdQuery(task.DownloadTaskId));
            if (rootDownloadTaskIdResult.IsFailed)
            {
                rootDownloadTaskIdResult.LogError();
                return;
            }

            var updateResult = await _mediator.Send(new UpdateRootDownloadStatusOfDownloadTaskCommand(rootDownloadTaskIdResult.Value));
            if (updateResult.IsFailed)
            {
                updateResult.LogError();
            }

            await _downloadProgressScheduler.FireDownloadProgressJob(task.PlexServerId);
        }

        private async Task<Result> UpdateDownloadTaskAsync(DownloadTask downloadTask)
        {
            var updateResult = await _mediator.Send(new UpdateDownloadTasksByIdCommand(new List<DownloadTask> { downloadTask }));
            if (updateResult.IsFailed)
            {
                updateResult.LogError();
            }

            return updateResult;
        }

        private async Task UpdateDownloadTasksAsync(List<DownloadTask> downloadTasks)
        {
            var updateResult = await _mediator.Send(new UpdateDownloadTasksByIdCommand(downloadTasks));
            if (updateResult.IsFailed)
            {
                updateResult.LogError();
            }
        }

        private async Task OnFileMergeProgress(FileMergeProgress progress)
        {
            Log.Debug(
                $"Merge Progress: {progress.DataTransferred} / {progress.DataTotal} - {progress.Percentage} - {progress.TransferSpeedFormatted}");

            var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(progress.DownloadTaskId));
            if (downloadTaskResult.IsFailed)
            {
                downloadTaskResult.LogError();
                return;
            }

            var downloadTask = downloadTaskResult.Value;
            downloadTask.Percentage = progress.Percentage;
            downloadTask.DataReceived = progress.DataTransferred;
            downloadTask.DownloadSpeed = progress.TransferSpeed;

            if (progress.Percentage >= 100)
            {
                downloadTask.DownloadStatus = DownloadStatus.Completed;
            }

            await UpdateDownloadTaskAsync(downloadTask);
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
            await _downloadQueue.CheckDownloadQueue(new List<int> { downloadTask.PlexServerId });
        }
    }
}