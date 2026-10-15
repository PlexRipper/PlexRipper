namespace Reaparr.Domain;

public class PlexMusicAlbumMediaQuality : BaseEntity
{
    [Column(Order = 1)]
    public required int PlexLibraryId { get; set; }

    [Column(Order = 2)]
    public required int PlexMusicAlbumId { get; set; }

    [Column(Order = 3)]
    public required AudioQuality AudioQuality { get; set; }

    public PlexLibrary? PlexLibrary { get; set; }

    public PlexMusicAlbum? PlexMusicAlbum { get; set; }

    [NotMapped]
    public PlexMediaType Type => PlexMediaType.MusicAlbum;
}
