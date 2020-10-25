using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexMovies
{
    public class GetPlexMovieByIdQuery : IRequest<Result<PlexMovie>>
    {
        public GetPlexMovieByIdQuery(int id, bool includePlexLibrary = false, bool includePlexServer = false)
        {
            Id = id;
            IncludePlexLibrary = includePlexLibrary;
            IncludePlexServer = includePlexServer;
        }

        public int Id { get; }

        public bool IncludePlexLibrary { get; }

        public bool IncludePlexServer { get; }
    }
}