using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class AddDownloadWorkerTasksCommand : IRequest<Result<bool>>
    {
        public List<DownloadWorkerTask> DownloadWorkerTasks { get; }

        public AddDownloadWorkerTasksCommand(List<DownloadWorkerTask> downloadWorkerTasks)
        {
            DownloadWorkerTasks = downloadWorkerTasks;
        }
    }
}