using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class AddFileTaskFromDownloadTaskCommand : IRequest<Result<int>>
{
    public AddFileTaskFromDownloadTaskCommand(DownloadTask downloadTask)
    {
        DownloadTask = downloadTask;
    }

    public DownloadTask DownloadTask { get; }
}