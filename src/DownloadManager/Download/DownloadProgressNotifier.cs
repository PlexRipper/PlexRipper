using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager
{
    public class DownloadProgressNotifier : IDownloadProgressNotifier
    {
        private readonly IMediator _mediator;

        private readonly ISignalRService _signalRService;

        public DownloadProgressNotifier(IMediator mediator, ISignalRService signalRService)
        {
            _mediator = mediator;
            _signalRService = signalRService;
        }

        public async Task<Result> SendDownloadProgress()
        {
            var downloadServers = await _mediator.Send(new GetAllDownloadTasksInPlexServersQuery());
            if (downloadServers.IsFailed)
            {
                return downloadServers;
            }

            foreach (var server in downloadServers.Value)
            {
                await SendDownloadProgress(server);
            }

            return Result.Ok();
        }

        public async Task SendDownloadProgress(PlexServer plexServer)
        {
            if (plexServer.HasDownloadTasks)
            {
                await _signalRService.SendDownloadProgressUpdate(plexServer.Id, plexServer.DownloadTasks);
            }
        }

        public async Task SendDownloadProgress(int plexServerId)
        {
            var downloadTasksResult =  await _mediator.Send(new GetAllDownloadTasksInPlexServerByIdQuery(plexServerId));
            if (downloadTasksResult.IsSuccess)
            {
                await _signalRService.SendDownloadProgressUpdate(plexServerId, downloadTasksResult.Value);
            }
        }
    }
}