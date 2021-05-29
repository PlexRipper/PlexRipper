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

        public Domain.DownloadTask DownloadTask { get; }
    }
}