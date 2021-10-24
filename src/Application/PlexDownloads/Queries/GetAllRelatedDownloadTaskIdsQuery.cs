using System.Collections.Generic;
using FluentResults;
using MediatR;

namespace PlexRipper.Application
{
    public class GetAllRelatedDownloadTaskIds : IRequest<Result<List<int>>>
    {
        public GetAllRelatedDownloadTaskIds(List<int> downloadTaskIds)
        {
            DownloadTaskIds = downloadTaskIds;
        }

        public List<int> DownloadTaskIds { get; }
    }
}