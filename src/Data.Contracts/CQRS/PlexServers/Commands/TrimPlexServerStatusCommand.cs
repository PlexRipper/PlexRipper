using FluentResults;
using MediatR;

namespace Data.Contracts;

public class TrimPlexServerStatusCommand : IRequest<Result<bool>>
{
    public int PlexServerId { get; }

    public TrimPlexServerStatusCommand(int plexServerId = 0)
    {
        PlexServerId = plexServerId;
    }
}