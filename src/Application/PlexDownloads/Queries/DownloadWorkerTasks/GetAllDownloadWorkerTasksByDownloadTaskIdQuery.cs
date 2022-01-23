using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.DownloadWorkerTasks
{
    public class GetAllDownloadWorkerTasksByDownloadTaskIdQuery : IRequest<Result<List<DownloadWorkerTask>>>
    {
        public GetAllDownloadWorkerTasksByDownloadTaskIdQuery(int downloadTaskId)
        {
            DownloadTaskId = downloadTaskId;
        }

        public int DownloadTaskId { get; }
    }
}