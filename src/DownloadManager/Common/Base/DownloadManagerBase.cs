using System.Threading.Tasks;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexDownloads;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager.Common
{
    public class DownloadManagerBase
    {
        protected readonly IMediator _mediator;

        protected readonly ISignalRService _signalRService;

        public DownloadManagerBase(IMediator mediator, ISignalRService signalRService)
        {
            _mediator = mediator;
            _signalRService = signalRService;
        }

        protected void SetDownloadStatus(int downloadTaskId, DownloadStatus status)
        {
            Task.Run(() => SetDownloadStatusAsync(downloadTaskId, status));
        }

        protected async Task SetDownloadStatusAsync(int downloadTaskId, DownloadStatus status)
        {
            Log.Debug($"DownloadClient changed downloadStatus for downloadTask {downloadTaskId} " +
                      $"to {status.ToString()}");

            await _mediator.Send(new UpdateDownloadStatusOfDownloadTaskCommand(downloadTaskId, status));
            await _signalRService.SendDownloadStatusUpdate(downloadTaskId, status);
        }
    }
}