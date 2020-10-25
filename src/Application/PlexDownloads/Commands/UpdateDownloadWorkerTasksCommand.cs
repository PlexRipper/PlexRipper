using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads.Commands
{
    public class UpdateDownloadWorkerTasksCommand : IRequest<Result<bool>>
    {
        public UpdateDownloadWorkerTasksCommand(IList<DownloadWorkerTask> downloadTasks)
        {
            DownloadTasks = downloadTasks;
        }

        public IList<DownloadWorkerTask> DownloadTasks { get; }
    }
}