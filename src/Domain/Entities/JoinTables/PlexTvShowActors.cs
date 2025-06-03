using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PlexRipper.Domain;

public class PlexTvShowActors
{
    public PlexTvShowActors() { }

    [SetsRequiredMembers]
    public PlexTvShowActors(int plexActorId, int plexLibraryId, int plexTvShowId, string roleName)
    {
        PlexActorId = plexActorId;
        PlexTvShowId = plexTvShowId;
        PlexLibraryId = plexLibraryId;
        RoleName = roleName;
    }

    [Column(Order = 1)]
    public required int PlexActorId { get; init; }

    [Column(Order = 2)]
    public required int PlexLibraryId { get; init; }

    [Column(Order = 3)]
    public required int PlexTvShowId { get; init; }

    [Column(Order = 4)]
    public required string RoleName { get; init; }
}
