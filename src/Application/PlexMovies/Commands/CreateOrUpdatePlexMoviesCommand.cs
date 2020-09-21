using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexMovies
{
    public class CreateOrUpdatePlexMoviesCommand : IRequest<Result<bool>>
    {
        public CreateOrUpdatePlexMoviesCommand(PlexLibrary plexLibrary, List<PlexMovie> plexMovies)
        {
            PlexLibrary = plexLibrary;
            PlexMovies = plexMovies;
        }

        public PlexLibrary PlexLibrary { get; }

        public List<PlexMovie> PlexMovies { get; }
    }
}