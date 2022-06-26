using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class UpdateDownloadStatusOfDownloadTaskCommand : IRequest<Result>
    {
        public UpdateDownloadStatusOfDownloadTaskCommand(List<int> downloadTaskIds, DownloadStatus downloadStatus)
        {
            DownloadTaskIds = downloadTaskIds;
            DownloadStatus = downloadStatus;
        }

        public List<int> DownloadTaskIds { get; }

        public DownloadStatus DownloadStatus { get; }
    }
}