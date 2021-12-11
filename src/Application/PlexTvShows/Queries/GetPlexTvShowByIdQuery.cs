using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class GetPlexTvShowByIdQuery : IRequest<Result<PlexTvShow>>
    {
        public GetPlexTvShowByIdQuery(int id, bool includeLibrary = false, bool includeServer = false)
        {
            Id = id;
            IncludeLibrary = includeLibrary;
            IncludeServer = includeServer;
        }

        public int Id { get; }

        public bool IncludeLibrary { get; }

        public bool IncludeServer { get; }
    }
}