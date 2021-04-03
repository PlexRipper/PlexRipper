using System.Threading.Tasks;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.DTO.DownloadManager;
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

        protected async Task UpdateDownloadTaskStatusAsync(DownloadClientUpdate downloadClientUpdate)
        {
            Log.Debug($"DownloadClient changed downloadStatus for downloadTask {downloadClientUpdate.Id} " +
                      $"to {downloadClientUpdate.DownloadStatus.ToString()}");

            await _mediator.Send(new UpdateDownloadTaskByIdCommand(downloadClientUpdate.DownloadTask));
            await _signalRService.SendDownloadTaskUpdate(downloadClientUpdate);
        }
    }
}