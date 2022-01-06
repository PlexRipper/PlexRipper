using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class PlexDownloadService : IPlexDownloadService
    {
        #region Fields

        private readonly IDownloadTaskFactory _downloadTaskFactory;

        private readonly IDownloadCommands _downloadCommands;

        private readonly IMediator _mediator;

        private readonly IDownloadQueue _downloadQueue;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlexDownloadService"/> class.
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="downloadManager"></param>
        /// <param name="downloadTaskFactory"></param>
        /// <param name="downloadCommands"></param>
        public PlexDownloadService(
            IMediator mediator,
            IDownloadQueue downloadQueue,
            IDownloadTaskFactory downloadTaskFactory,
            IDownloadCommands downloadCommands)
        {
            _mediator = mediator;
            _downloadQueue = downloadQueue;
            _downloadTaskFactory = downloadTaskFactory;
            _downloadCommands = downloadCommands;
        }

        #endregion

        #region Methods

        #region Public

        public async Task<Result<List<DownloadTask>>> GetDownloadTasksAsync()
        {
            return await _mediator.Send(new GetAllDownloadTasksQuery());
        }

        #region Commands

        public async Task<Result> DownloadMediaAsync(List<DownloadMediaDTO> downloadTaskOrders)
        {
            Log.Debug($"Attempting to add download task orders: {downloadTaskOrders}");
            var downloadTasks = await _downloadTaskFactory.GenerateAsync(downloadTaskOrders);
            if (downloadTasks.IsFailed)
                return downloadTasks.ToResult();

            // Sent to download manager
            return await _downloadQueue.AddToDownloadQueueAsync(downloadTasks.Value);
        }

        public async Task<Result> DeleteDownloadTasksAsync(List<int> downloadTaskIds)
        {
            return await _downloadCommands.DeleteDownloadTaskClientsAsync(downloadTaskIds);
        }

        public Task<Result> RestartDownloadTask(int downloadTaskId)
        {
            return _downloadCommands.RestartDownloadTasksAsync(downloadTaskId);
        }

        public async Task<Result> StopDownloadTask(int downloadTaskId)
        {
            var result = await _downloadCommands.StopDownloadTasksAsync(downloadTaskId);
            return result.IsSuccess ? Result.Ok() : result.ToResult();
        }

        public Task<Result> StartDownloadTask(int downloadTaskId)
        {
            return _downloadCommands.ResumeDownloadTasksAsync(downloadTaskId);
        }

        public Task<Result> PauseDownloadTask(int downloadTaskId)
        {
            return _downloadCommands.PauseDownload(downloadTaskId);
        }

        public Task<Result> ClearCompleted(List<int> downloadTaskIds)
        {
            return _downloadCommands.ClearCompletedAsync(downloadTaskIds);
        }

        #endregion

        #endregion

        #endregion
    }
}