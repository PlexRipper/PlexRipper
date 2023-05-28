using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class AddDownloadWorkerTasksCommand : IRequest<Result<bool>>
{
    public List<DownloadWorkerTask> DownloadWorkerTasks { get; }

    public AddDownloadWorkerTasksCommand(List<DownloadWorkerTask> downloadWorkerTasks)
    {
        DownloadWorkerTasks = downloadWorkerTasks;
    }
}