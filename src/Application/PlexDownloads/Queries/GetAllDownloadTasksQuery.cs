using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads
{
    public class GetAllDownloadTasksQuery : IRequest<Result<List<DownloadTask>>>
    {
        public GetAllDownloadTasksQuery(bool includeServer = false, bool includePlexLibrary = false)
        {
            IncludeServer = includeServer;
            IncludePlexLibrary = includePlexLibrary;
        }

        public bool IncludeServer { get; }

        public bool IncludePlexLibrary { get; }
    }
}