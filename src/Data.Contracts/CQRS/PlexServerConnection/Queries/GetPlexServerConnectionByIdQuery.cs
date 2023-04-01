using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetPlexServerConnectionByIdQuery : IRequest<Result<PlexServerConnection>>
{
    public GetPlexServerConnectionByIdQuery(int id, bool includeServer = false)
    {
        Id = id;
        IncludeServer = includeServer;
    }

    public int Id { get; }

    public bool IncludeServer { get; }
}