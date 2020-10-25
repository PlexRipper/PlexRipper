using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads.Commands
{
    public class AddDownloadTaskCommand : IRequest<Result<int>>
    {
        public DownloadTask DownloadTask { get; }

        public AddDownloadTaskCommand(DownloadTask downloadTask)
        {
            DownloadTask = downloadTask;
        }
    }
}