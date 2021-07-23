using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class GetDownloadTaskByIdQuery : IRequest<Result<DownloadTask>>
    {
        public GetDownloadTaskByIdQuery(int id, bool includeServer = false, bool includePlexLibrary = false)
        {
            Id = id;
            IncludeServer = includeServer;
            IncludePlexLibrary = includePlexLibrary;
        }

        public int Id { get; }

        public bool IncludeServer { get; }

        public bool IncludePlexLibrary { get; }
    }
}