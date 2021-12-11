using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class CreateUpdateOrDeletePlexMoviesCommand : IRequest<Result<bool>>
    {
        public CreateUpdateOrDeletePlexMoviesCommand(PlexLibrary plexLibrary)
        {
            PlexLibrary = plexLibrary;
        }

        public PlexLibrary PlexLibrary { get; }
    }
}