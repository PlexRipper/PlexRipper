using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads
{
    public class UpdateDownloadTaskByIdCommand : IRequest<Result<bool>>
    {
        public UpdateDownloadTaskByIdCommand(DownloadTask downloadTask)
        {
            DownloadTask = downloadTask;
        }

        public DownloadTask DownloadTask { get; }
    }
}