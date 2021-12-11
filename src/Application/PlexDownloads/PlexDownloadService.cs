using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.WebApi;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class PlexDownloadService : IPlexDownloadService
    {
        #region Fields

        private readonly IDownloadManager _downloadManager;

        private readonly IDownloadTaskFactory _downloadTaskFactory;

        private readonly IDownloadCommands _downloadCommands;

        private readonly IMediator _mediator;

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
            IDownloadManager downloadManager,
            IDownloadTaskFactory downloadTaskFactory,
            IDownloadCommands downloadCommands)
        {
            _mediator = mediator;
            _downloadManager = downloadManager;
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
            var downloadTasks = await _downloadTaskFactory.GenerateAsync(downloadTaskOrders);
            if (downloadTasks.IsFailed)
                return downloadTasks.ToResult();

            // Sent to download manager
            return await _downloadManager.AddToDownloadQueueAsync(downloadTasks.Value);
        }

        public async Task<Result> DeleteDownloadTasksAsync(List<int> downloadTaskIds)
        {
            return await _downloadCommands.DeleteDownloadTaskClientsAsync(downloadTaskIds);
        }

        public Task<Result> RestartDownloadTask(List<int> downloadTaskIds)
        {
            return _downloadCommands.RestartDownloadTasksAsync(downloadTaskIds);
        }

        public async Task<Result> StopDownloadTask(List<int> downloadTaskIds)
        {
            var result = await _downloadCommands.StopDownloadTasksAsync(downloadTaskIds);
            return result.IsSuccess ? Result.Ok() : result.ToResult();
        }

        public Task<Result> StartDownloadTask(List<int> downloadTaskIds)
        {
            return _downloadCommands.ResumeDownloadTasksAsync(downloadTaskIds);
        }

        public Task<Result> PauseDownloadTask(List<int> downloadTaskIds)
        {
            return _downloadCommands.PauseDownload(downloadTaskIds);
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