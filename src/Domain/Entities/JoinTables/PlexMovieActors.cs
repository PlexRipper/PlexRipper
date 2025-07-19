using System.Diagnostics.CodeAnalysis;

namespace PlexRipper.Domain;

public class PlexMovieActors
{
    public PlexMovieActors() { }

    [SetsRequiredMembers]
    public PlexMovieActors(int plexActorId, int plexLibraryId, int plexMovieId)
    {
        PlexActorId = plexActorId;
        PlexMovieId = plexMovieId;
        PlexLibraryId = plexLibraryId;
    }

    [Column(Order = 1)]
    public required int PlexActorId { get; set; }

    [Column(Order = 2)]
    public required int PlexMovieId { get; set; }

    [Column(Order = 3)]
    public required int PlexLibraryId { get; set; }
}
