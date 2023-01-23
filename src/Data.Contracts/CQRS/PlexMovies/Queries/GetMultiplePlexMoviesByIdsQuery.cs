using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetMultiplePlexMoviesByIdsQuery : IRequest<Result<List<PlexMovie>>>
{
    public GetMultiplePlexMoviesByIdsQuery(List<int> ids, bool includeLibrary = false, bool includeServer = false)
    {
        Ids = ids;
        IncludeLibrary = includeLibrary;
        IncludeServer = includeServer;
    }

    public List<int> Ids { get; }

    public bool IncludeLibrary { get; }

    public bool IncludeServer { get; }
}