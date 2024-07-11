using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetPlexTvShowByIdWithSeasonsQuery : IRequest<Result<PlexTvShow>>
{
    public GetPlexTvShowByIdWithSeasonsQuery(int id, bool includePlexServer = false, bool includePlexLibrary = false)
    {
        Id = id;
        IncludePlexServer = includePlexServer;
        IncludePlexLibrary = includePlexLibrary;
    }

    public int Id { get; }

    public bool IncludePlexServer { get; }

    public bool IncludePlexLibrary { get; }
}
