using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexTvShows
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