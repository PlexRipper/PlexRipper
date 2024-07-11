using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetPlexTvShowsByPlexLibraryIdQuery : IRequest<Result<List<PlexTvShow>>>
{
    public GetPlexTvShowsByPlexLibraryIdQuery(int plexLibraryId)
    {
        PlexLibraryId = plexLibraryId;
    }

    public int PlexLibraryId { get; }
}
