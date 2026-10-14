namespace Reaparr.Domain;

public static class PlexMediaTypeExtensions
{
    private static readonly IReadOnlyList<PlexMediaType> _movieTypes = [PlexMediaType.Movie];
    private static readonly IReadOnlyList<PlexMediaType> _tvShowTypes =
    [
        PlexMediaType.TvShow,
        PlexMediaType.Season,
        PlexMediaType.Episode,
    ];
    private static readonly IReadOnlyList<PlexMediaType> _musicTypes =
    [
        PlexMediaType.MusicArtist,
        PlexMediaType.MusicAlbum,
        PlexMediaType.MusicTrack,
    ];
    private static readonly IReadOnlyList<PlexMediaType> _photoTypes =
    [
        PlexMediaType.PhotoAlbum,
        PlexMediaType.PhotoImage,
    ];
    private static readonly IReadOnlyList<PlexMediaType> _otherVideoTypes = [PlexMediaType.OtherVideos];

    public static bool IsRootType(this PlexMediaType mediaType) =>
        mediaType
            is PlexMediaType.Movie
                or PlexMediaType.TvShow
                or PlexMediaType.MusicArtist
                or PlexMediaType.PhotoAlbum
                or PlexMediaType.OtherVideos;

    public static bool IsTvType(this PlexMediaType mediaType) => _tvShowTypes.Contains(mediaType);

    public static bool IsMovieType(this PlexMediaType mediaType) => _movieTypes.Contains(mediaType);

    public static bool IsMusicType(this PlexMediaType mediaType) => _musicTypes.Contains(mediaType);

    public static bool IsPhotoType(this PlexMediaType mediaType) => _photoTypes.Contains(mediaType);

    public static bool IsOtherVideosType(this PlexMediaType mediaType) => _otherVideoTypes.Contains(mediaType);

    public static bool SupportsComparison(this PlexMediaType downloadTaskType) =>
        downloadTaskType is PlexMediaType.Movie or PlexMediaType.TvShow or PlexMediaType.MusicArtist;

    public static IReadOnlyList<PlexMediaType> GetFamilyTypes(this PlexMediaType libraryType) =>
        libraryType switch
        {
            PlexMediaType.Movie => _movieTypes,
            PlexMediaType.TvShow => _tvShowTypes,
            PlexMediaType.MusicArtist => _musicTypes,
            PlexMediaType.PhotoAlbum => _photoTypes,
            PlexMediaType.OtherVideos => _otherVideoTypes,
            _ => [],
        };

    public static int ToDefaultDestinationFolderId(this PlexMediaType type)
    {
        return type switch
        {
            PlexMediaType.Movie => 2,
            PlexMediaType.TvShow or PlexMediaType.Season or PlexMediaType.Episode => 3,
            PlexMediaType.MusicArtist or PlexMediaType.MusicAlbum or PlexMediaType.MusicTrack => 4,
            PlexMediaType.PhotoImage or PlexMediaType.PhotoAlbum => 5,
            PlexMediaType.OtherVideos => 6,
            PlexMediaType.Games => 7,
            _ => FolderTypeDefaults.DefaultDownloadFolderId, // Used for Downloads folder
        };
    }
}
