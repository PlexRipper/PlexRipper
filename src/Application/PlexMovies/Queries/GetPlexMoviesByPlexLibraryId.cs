using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class GetPlexMoviesByPlexLibraryId : IRequest<Result<List<PlexMovie>>>
    {
        public GetPlexMoviesByPlexLibraryId(int plexLibraryId)
        {
            PlexLibraryId = plexLibraryId;
        }

        public int PlexLibraryId { get; }
    }
}