using System.Threading.Tasks;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Domain;
using PlexRipper.Domain.RxNet;

namespace PlexRipper.DownloadManager
{
    public class DownloadStatusManager
    {
        private readonly IMediator _mediator;

        private readonly IDownloadTracker _downloadTracker;

        public DownloadStatusManager(IMediator mediator, IDownloadTracker downloadTracker)
        {
            _mediator = mediator;
            _downloadTracker = downloadTracker;
            SetupSubscriptions();
        }

        public void SetupSubscriptions()
        {
            // Start sending updates to the front-end when starting a DownloadTask
            _downloadTracker
                .DownloadTaskStart
                .SubscribeAsync(async downloadTask => await OnStarted(downloadTask));
        }

        public async Task OnStarted(DownloadTask downloadTask)
        {
            if (downloadTask.RootDownloadTaskId is null)
                return;

            var rootDownloadTaskById = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTask.RootDownloadTaskId ?? 0));

        }
    }
}