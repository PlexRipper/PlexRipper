using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexTvShows.Queries
{
    public class GetPlexTvShowsByPlexLibraryId : IRequest<Result<List<PlexTvShow>>>
    {
        public GetPlexTvShowsByPlexLibraryId(int plexLibraryId)
        {
            PlexLibraryId = plexLibraryId;
        }

        public int PlexLibraryId { get; }
    }
}