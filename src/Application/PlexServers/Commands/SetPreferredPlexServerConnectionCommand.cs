﻿namespace PlexRipper.Application;

public class SetPreferredPlexServerConnectionCommand : IRequest<Result>
{
    public SetPreferredPlexServerConnectionCommand(int plexServerId, int plexServerConnectionId)
    {
        PlexServerId = plexServerId;
        PlexServerConnectionId = plexServerConnectionId;
    }

    public int PlexServerId { get; }

    public int PlexServerConnectionId { get; }
}