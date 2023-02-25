using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class UpdateDownloadTasksByIdCommand : IRequest<Result>
{
    public List<DownloadTask> DownloadTasks { get; }

    public UpdateDownloadTasksByIdCommand(DownloadTask downloadTask)
    {
        DownloadTasks = new List<DownloadTask>() { downloadTask };
    }

    public UpdateDownloadTasksByIdCommand(List<DownloadTask> downloadTasks)
    {
        DownloadTasks = downloadTasks;
    }
}