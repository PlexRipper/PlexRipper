using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class CreateUpdateOrDeletePlexTvShowsCommand : IRequest<Result<bool>>
    {
        public CreateUpdateOrDeletePlexTvShowsCommand(PlexLibrary plexLibrary)
        {
            PlexLibrary = plexLibrary;
        }

        public PlexLibrary PlexLibrary { get; }
    }
}