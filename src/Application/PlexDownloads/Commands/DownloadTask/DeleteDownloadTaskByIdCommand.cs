using FluentResults;
using MediatR;

namespace PlexRipper.Application.PlexDownloads
{
    public class DeleteDownloadTaskByIdCommand : IRequest<Result<bool>>
    {
        public int DownloadTaskId { get; }

        public DeleteDownloadTaskByIdCommand(int downloadTaskId)
        {
            DownloadTaskId = downloadTaskId;
        }
    }
}