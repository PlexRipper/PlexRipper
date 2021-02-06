using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads
{
    public class UpdateDownloadTaskByIdCommand : IRequest<Result>
    {
        public UpdateDownloadTaskByIdCommand(DownloadTask downloadTask)
        {
            DownloadTask = downloadTask;
        }

        public DownloadTask DownloadTask { get; }
    }
}