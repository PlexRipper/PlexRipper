using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{

    public class GetAllRelatedDownloadTaskIds : IRequest<Result<List<int>>>
    {
        /// <summary>
        /// Creates a flattened list of <see cref="DownloadTask">DownloadTaskIds</see> which are a child of the ids that are given.
        /// </summary>
        /// <param name="downloadTaskIds"></param>
        public GetAllRelatedDownloadTaskIds(List<int> downloadTaskIds)
        {
            DownloadTaskIds = downloadTaskIds;
        }

        public List<int> DownloadTaskIds { get; }
    }
}