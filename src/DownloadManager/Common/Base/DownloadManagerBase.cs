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

        protected void SetDownloadStatus(DownloadStatusChanged downloadStatusChanged)
        {
            Task.Run(() => SetDownloadStatusAsync(downloadStatusChanged));
        }

        protected async Task SetDownloadStatusAsync(DownloadStatusChanged downloadStatusChanged)
        {
            Log.Debug($"DownloadClient changed downloadStatus for downloadTask {downloadStatusChanged.Id} " +
                      $"to {downloadStatusChanged.Status.ToString()}");

            await _mediator.Send(new UpdateDownloadStatusOfDownloadTaskCommand(downloadStatusChanged.Id, downloadStatusChanged.Status));
            await _signalRService.SendDownloadStatusUpdate(downloadStatusChanged.Id, downloadStatusChanged.Status, downloadStatusChanged.PlexServerId,
                downloadStatusChanged.PlexLibraryId);
        }
    }
}