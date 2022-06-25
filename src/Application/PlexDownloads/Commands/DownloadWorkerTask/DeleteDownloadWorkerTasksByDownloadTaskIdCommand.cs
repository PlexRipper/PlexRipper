using FluentResults;
using MediatR;

namespace PlexRipper.Application
{
    public class DeleteDownloadWorkerTasksByDownloadTaskIdCommand : IRequest<Result>
    {
        public int DownloadTaskId { get; }

        public DeleteDownloadWorkerTasksByDownloadTaskIdCommand(int downloadTaskId)
        {
            DownloadTaskId = downloadTaskId;
        }
    }
}