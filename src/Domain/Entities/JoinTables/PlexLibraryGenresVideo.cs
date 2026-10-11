namespace Reaparr.Domain;

public class PlexLibraryGenresVideo
{
    public PlexLibraryGenresVideo() { }

    [SetsRequiredMembers]
    public PlexLibraryGenresVideo(int plexLibraryId, int plexGenreId)
    {
        PlexLibraryId = plexLibraryId;
        PlexGenreId = plexGenreId;
    }

    [Column(Order = 1)]
    public required int PlexLibraryId { get; set; }

    [Column(Order = 2)]
    public required int PlexGenreId { get; set; }
}
