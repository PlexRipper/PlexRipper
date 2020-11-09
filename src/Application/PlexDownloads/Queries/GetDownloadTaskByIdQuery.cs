using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads
{
    public class GetDownloadTaskByIdQuery : IRequest<Result<DownloadTask>>
    {
        public GetDownloadTaskByIdQuery(int id, bool includeServer = false, bool includePlexAccount = false, bool includePlexLibrary = false)
        {
            Id = id;
            IncludeServer = includeServer;
            IncludePlexAccount = includePlexAccount;
            IncludePlexLibrary = includePlexLibrary;
        }

        public int Id { get; }

        public bool IncludeServer { get; }

        public bool IncludePlexAccount { get; }

        public bool IncludePlexLibrary { get; }
    }
}