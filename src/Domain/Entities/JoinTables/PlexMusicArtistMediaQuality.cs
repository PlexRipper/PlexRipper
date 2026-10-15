namespace Reaparr.Domain;

public class PlexMusicArtistMediaQuality : BaseEntity
{
    [Column(Order = 1)]
    public required int PlexLibraryId { get; set; }

    [Column(Order = 2)]
    public required int PlexMusicArtistId { get; set; }

    [Column(Order = 3)]
    public required AudioQuality AudioQuality { get; set; }

    public PlexLibrary? PlexLibrary { get; set; }

    public PlexMusicArtist? PlexMusicArtist { get; set; }

    [NotMapped]
    public PlexMediaType Type => PlexMediaType.MusicArtist;
}
