using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.FileManager.Command
{
    public class AddFileTaskFromDownloadTaskCommand : IRequest<Result<int>>
    {
        public AddFileTaskFromDownloadTaskCommand(DownloadTask downloadTask)
        {
            DownloadTask = downloadTask;
        }

        public DownloadTask DownloadTask { get; }
    }
}