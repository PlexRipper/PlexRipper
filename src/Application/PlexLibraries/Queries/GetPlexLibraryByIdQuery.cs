using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexLibraries.Queries
{
    public class GetPlexLibraryByIdQuery : IRequest<Result<PlexLibrary>>
    {
        public GetPlexLibraryByIdQuery(int id, bool includePlexServer = false, bool includeMedia = false)
        {
            Id = id;
            IncludePlexServer = includePlexServer;
            IncludeMedia = includeMedia;
        }

        public int Id { get; }

        public bool IncludePlexServer { get; }

        public bool IncludeMedia { get; }
    }
}