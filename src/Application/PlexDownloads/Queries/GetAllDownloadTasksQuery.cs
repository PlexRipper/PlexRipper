using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads
{
    public class GetAllDownloadTasksQuery : IRequest<Result<List<DownloadTask>>>
    {
        public GetAllDownloadTasksQuery(List<int> downloadTaskIds = null, bool includeServer = false, bool includePlexLibrary = false)
        {
            DownloadTaskIds = downloadTaskIds;
            IncludeServer = includeServer;
            IncludePlexLibrary = includePlexLibrary;
        }

        public List<int> DownloadTaskIds { get; }

        public bool IncludeServer { get; }

        public bool IncludePlexLibrary { get; }
    }
}