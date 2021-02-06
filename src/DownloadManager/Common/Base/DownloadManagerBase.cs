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

        protected async Task UpdateDownloadTaskStatusAsync(DownloadTask downloadTask)
        {
            Log.Debug($"DownloadClient changed downloadStatus for downloadTask {downloadTask.Id} " +
                      $"to {downloadTask.DownloadStatus.ToString()}");

            await _mediator.Send(new UpdateDownloadTaskByIdCommand(downloadTask));
            await _signalRService.SendDownloadTaskUpdate(downloadTask);
        }
    }
}