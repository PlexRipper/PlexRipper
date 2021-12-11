using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Domain;
using PlexRipper.Domain.RxNet;

namespace PlexRipper.DownloadManager
{
    /// <summary>
    /// Handles all <see cref="DownloadTask"/> management, all download related commands should be handled here.
    /// </summary>
    public class DownloadManager : IDownloadManager
    {
        #region Fields

        private readonly IMediator _mediator;

        private readonly IDownloadTaskValidator _downloadTaskValidator;

        private readonly IDownloadQueue _downloadQueue;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadManager"/> class.
        /// </summary>
        /// <param name="mediator">Defines a mediator to encapsulate request/response and publishing interaction patterns.</param>
        /// <param name="downloadQueue">Used to retrieve the next <see cref="DownloadTask"/> from the <see cref="DownloadQueue"/>.</param>
        /// <param name="downloadTaskValidator"></param>
        public DownloadManager(
            IMediator mediator,
            IDownloadQueue downloadQueue,
            IDownloadTaskValidator downloadTaskValidator
        )
        {
            _mediator = mediator;
            _downloadTaskValidator = downloadTaskValidator;
            _downloadQueue = downloadQueue;

            SetupSubscriptions();
        }

        #endregion

        #region Methods

        #region Private

        private void SetupSubscriptions()
        {
            _downloadQueue
                .UpdateDownloadTasks
                .SubscribeAsync(UpdateDownloadTasksAsync);
        }

        #region Subscriptions

        private async Task UpdateDownloadTasksAsync(List<DownloadTask> downloadTasks)
        {
            var updateResult = await _mediator.Send(new UpdateDownloadTasksByIdCommand(downloadTasks));
            if (updateResult.IsFailed)
            {
                updateResult.LogError();
            }
        }

        #endregion

        #endregion

        #region Public

        #region Commands

        /// <inheritdoc/>
        public async Task<Result> AddToDownloadQueueAsync(List<DownloadTask> downloadTasks)
        {
            if (!downloadTasks.Any())
                return ResultExtensions.IsEmpty(nameof(downloadTasks)).LogWarning();

            Log.Debug($"Attempt to add {downloadTasks.Count} DownloadTasks");
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

            if (createResult.Value.Count != validateResult.Value.Count)
            {
                return Result.Fail("The added download tasks are not stored correctly, missing download tasks.").LogError();
            }

            Log.Debug($"Successfully added all {validateResult.Value.Count} DownloadTasks");
            await _downloadQueue.CheckDownloadQueue();
            return Result.Ok();
        }

        #endregion

        #endregion

        #endregion
    }
}