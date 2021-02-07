using FluentResults;
using MediatR;

namespace PlexRipper.Application.PlexDownloads
{
    public class UpdateDownloadTaskByIdCommand : IRequest<Result>
    {
        public UpdateDownloadTaskByIdCommand(Domain.DownloadTask downloadTask)
        {
            DownloadTask = downloadTask;
        }

        public Domain.DownloadTask DownloadTask { get; }
    }
}