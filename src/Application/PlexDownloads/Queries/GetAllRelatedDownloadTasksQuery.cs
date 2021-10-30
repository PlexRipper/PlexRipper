using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{

    public class GetAllRelatedDownloadTasksQuery : IRequest<Result<List<DownloadTask>>>
    {
        /// <summary>
        /// Creates a flattened list of <see cref="DownloadTask">DownloadTasks</see> which are a child of the ids that are given.
        /// </summary>
        /// <param name="downloadTaskIds"></param>
        public GetAllRelatedDownloadTasksQuery(List<int> downloadTaskIds)
        {
            DownloadTaskIds = downloadTaskIds;
        }

        public List<int> DownloadTaskIds { get; }
    }
}