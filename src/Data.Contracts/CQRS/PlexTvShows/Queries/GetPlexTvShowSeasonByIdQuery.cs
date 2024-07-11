using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetPlexTvShowSeasonByIdQuery : IRequest<Result<PlexTvShowSeason>>
{
    public GetPlexTvShowSeasonByIdQuery(int id, bool includeLibrary = false, bool includeServer = false)
    {
        Id = id;
        IncludeLibrary = includeLibrary;
        IncludeServer = includeServer;
    }

    public int Id { get; }

    public bool IncludeLibrary { get; }

    public bool IncludeServer { get; }
}
