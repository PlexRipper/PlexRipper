using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class GetPlexServerByPlexLibraryIdQuery : IRequest<Result<PlexServer>>
    {
        public GetPlexServerByPlexLibraryIdQuery(int id, bool includePlexLibraries = false)
        {
            Id = id;
            IncludePlexLibraries = includePlexLibraries;
        }

        public int Id { get; }

        public bool IncludePlexLibraries { get; }
    }
}