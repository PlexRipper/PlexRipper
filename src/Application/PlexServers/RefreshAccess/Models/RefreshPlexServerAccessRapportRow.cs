using System.Diagnostics.CodeAnalysis;

namespace PlexRipper.Application;

public record RefreshPlexServerAccessRapportRow
{
    [SetsRequiredMembers]
    public RefreshPlexServerAccessRapportRow(PlexAccessState state, int plexServerId, string plexServerName)
    {
        State = state;
        PlexServerId = plexServerId;
        PlexServerName = plexServerName;
    }

    public required int PlexServerId { get; set; }

    public required string PlexServerName { get; set; }

    public required PlexAccessState State { get; set; }
}
