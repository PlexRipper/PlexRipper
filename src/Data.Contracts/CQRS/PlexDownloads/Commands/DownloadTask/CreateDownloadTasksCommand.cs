using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class CreateDownloadTasksCommand : IRequest<Result>
{
    public List<DownloadTask> DownloadTasks { get; }

    public CreateDownloadTasksCommand(List<DownloadTask> downloadTasks)
    {
        DownloadTasks = downloadTasks;
    }
}