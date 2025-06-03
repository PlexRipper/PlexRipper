using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PlexRipper.Domain;

public class PlexMovieActors
{
    public PlexMovieActors() { }

    [SetsRequiredMembers]
    public PlexMovieActors(int plexActorId, int plexLibraryId, int plexMovieId, string roleName)
    {
        PlexActorId = plexActorId;
        PlexMovieId = plexMovieId;
        PlexLibraryId = plexLibraryId;
        RoleName = roleName;
    }

    [Column(Order = 1)]
    public required int PlexActorId { get; set; }

    [Column(Order = 2)]
    public required int PlexMovieId { get; set; }

    [Column(Order = 3)]
    public required int PlexLibraryId { get; set; }

    [Column(Order = 4)]
    public required string RoleName { get; set; }
}
