using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads.Queries
{
    public class GetAllDownloadTasksQuery : IRequest<Result<List<DownloadTask>>>
    {
        public GetAllDownloadTasksQuery(bool includeServer = false, bool includePlexAccount = false,
            bool includePlexLibrary = false)
        {
            IncludeServer = includeServer;
            IncludePlexAccount = includePlexAccount;
            IncludePlexLibrary = includePlexLibrary;
        }

        public bool IncludeServer { get; }

        public bool IncludePlexAccount { get; }

        public bool IncludePlexLibrary { get; }
    }
}