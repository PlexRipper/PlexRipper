using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class UpdateDownloadStatusOfDownloadTaskCommand : IRequest<Result<bool>>
    {
        public UpdateDownloadStatusOfDownloadTaskCommand(int downloadTaskId, DownloadStatus downloadStatus)
        {
            DownloadTaskId = downloadTaskId;
            DownloadStatus = downloadStatus;
        }

        public int DownloadTaskId { get; }

        public DownloadStatus DownloadStatus { get; }
    }
}