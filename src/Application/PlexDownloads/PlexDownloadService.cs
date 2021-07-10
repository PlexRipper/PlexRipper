using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.DTO.WebApi;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads
{
    public class PlexDownloadService : IPlexDownloadService
    {
        #region Fields

        private readonly IDownloadManager _downloadManager;

        private readonly INotificationsService _notificationsService;

        private readonly IPlexDownloadTaskFactory _plexDownloadTaskFactory;

        private readonly IMediator _mediator;

        private readonly ISignalRService _signalRService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlexDownloadService"/> class.
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="downloadManager"></param>
        /// <param name="signalRService"></param>
        /// <param name="notificationsService"></param>
        /// <param name="plexDownloadTaskFactory"></param>
        public PlexDownloadService(
            IMediator mediator,
            IDownloadManager downloadManager,
            ISignalRService signalRService,
            INotificationsService notificationsService,
            IPlexDownloadTaskFactory plexDownloadTaskFactory)
        {
            _mediator = mediator;
            _downloadManager = downloadManager;
            _signalRService = signalRService;
            _notificationsService = notificationsService;
            _plexDownloadTaskFactory = plexDownloadTaskFactory;
        }

        #endregion

        #region Methods

        #region Public

        public async Task<Result<List<DownloadTask>>> GetDownloadTasksAsync()
        {
            return await _mediator.Send(new GetAllDownloadTasksQuery());
        }

        #region Commands

        public async Task<Result> DownloadMediaAsync(List<DownloadMediaDTO> downloadMedias)
        {
            int mediaCount = downloadMedias.Select(x => x.MediaIds.Count).Sum();
            await _signalRService.SendDownloadTaskCreationProgressUpdate(1, mediaCount);
            int count = 0;

            List<DownloadTask> downloadTasks = new();

            foreach (var downloadMedia in downloadMedias)
            {
                var result = await _plexDownloadTaskFactory.GenerateAsync(downloadMedia.MediaIds, downloadMedia.Type);

                if (result.IsFailed)
                {
                    await _notificationsService.SendResult(result);
                }
                else
                {
                    downloadTasks.AddRange(result.Value);
                }

                count += downloadMedia.MediaIds.Count;
                await _signalRService.SendDownloadTaskCreationProgressUpdate(count, mediaCount);
            }

            await _signalRService.SendDownloadTaskCreationProgressUpdate(mediaCount, mediaCount);

            // Sent to download manager
            return await _downloadManager.AddToDownloadQueueAsync(downloadTasks);
        }

        public async Task<Result> DeleteDownloadTasksAsync(List<int> downloadTaskIds)
        {
            return await _downloadManager.DeleteDownloadTaskClientsAsync(downloadTaskIds);
        }

        public Task<Result> RestartDownloadTask(List<int> downloadTaskIds)
        {
            return _downloadManager.RestartDownloadTasksAsync(downloadTaskIds);
        }

        public async Task<Result> StopDownloadTask(List<int> downloadTaskIds)
        {
            var result = await _downloadManager.StopDownloadTasksAsync(downloadTaskIds);
            return result.IsSuccess ? Result.Ok() : result.ToResult();
        }

        public Task<Result> StartDownloadTask(List<int> downloadTaskIds)
        {
            return _downloadManager.ResumeDownloadTasksAsync(downloadTaskIds);
        }

        public Task<Result> PauseDownloadTask(List<int> downloadTaskIds)
        {
            return _downloadManager.PauseDownload(downloadTaskIds);
        }

        public Task<Result> ClearCompleted(List<int> downloadTaskIds)
        {
            return _downloadManager.ClearCompletedAsync(downloadTaskIds);
        }

        #endregion

        #endregion

        #region Private

        private Result<DownloadTask> PrioritizeDownloadTask(DownloadTask downloadTask)
        {
            // TODO This is intended to change the order of downloads, not finished
            downloadTask.Priority = DataFormat.GetPriority();
            return Result.Ok(downloadTask);
        }

        private Result<List<DownloadTask>> PrioritizeDownloadTasks(List<DownloadTask> downloadTasks)
        {
            // TODO This is intended to change the order of downloads, not finished
            var priorities = DataFormat.GetPriority(downloadTasks.Count);
            for (int i = 0; i < downloadTasks.Count; i++)
            {
                downloadTasks[i].Priority = priorities[i];
            }

            return Result.Ok(downloadTasks);
        }

        #endregion

        #endregion
    }
}