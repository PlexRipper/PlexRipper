using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class UpdateDownloadWorkerTasksCommand : IRequest<Result<bool>>
{
    public UpdateDownloadWorkerTasksCommand(IList<DownloadWorkerTask> downloadTasks)
    {
        DownloadTasks = downloadTasks;
    }

    public IList<DownloadWorkerTask> DownloadTasks { get; }
}