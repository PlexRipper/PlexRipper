using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PlexRipper.Domain;

public class PlexTvShowActors
{
    public PlexTvShowActors() { }

    [SetsRequiredMembers]
    public PlexTvShowActors(int plexActorId, int plexLibraryId, int plexTvShowId)
    {
        PlexActorId = plexActorId;
        PlexTvShowId = plexTvShowId;
        PlexLibraryId = plexLibraryId;
    }

    [Column(Order = 1)]
    public required int PlexActorId { get; init; }

    [Column(Order = 2)]
    public required int PlexLibraryId { get; init; }

    [Column(Order = 3)]
    public required int PlexTvShowId { get; init; }
}
