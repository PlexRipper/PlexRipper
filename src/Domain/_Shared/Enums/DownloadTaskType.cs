namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DownloadTaskType
{
    // NOTE: Keep these values contiguous. The TypeScript DTO translator depends on the ordering.
    [JsonStringEnumMemberName(nameof(None))]
    None = 0,

    /// <summary>
    /// Functions as a wrapper for a MovieData <see cref="DownloadTaskGeneric"/>.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Movie))]
    Movie = 1,

    /// <summary>
    /// A movie of a particular quality or version
    /// Doc: https://support.plex.tv/articles/200381043-multi-version-movies/.
    /// </summary>
    [JsonStringEnumMemberName(nameof(MovieData))]
    MovieData = 2,

    [JsonStringEnumMemberName(nameof(TvShow))]
    TvShow = 3,

    [JsonStringEnumMemberName(nameof(Season))]
    Season = 4,

    [JsonStringEnumMemberName(nameof(Episode))]
    Episode = 5,

    [JsonStringEnumMemberName(nameof(EpisodeData))]
    EpisodeData = 6,

    [JsonStringEnumMemberName(nameof(MusicArtist))]
    MusicArtist = 7,

    [JsonStringEnumMemberName(nameof(MusicAlbum))]
    MusicAlbum = 8,

    [JsonStringEnumMemberName(nameof(MusicTrack))]
    MusicTrack = 9,

    [JsonStringEnumMemberName(nameof(MusicTrackData))]
    MusicTrackData = 10,

    [JsonStringEnumMemberName(nameof(PhotoAlbum))]
    PhotoAlbum = 11,

    [JsonStringEnumMemberName(nameof(PhotoImage))]
    PhotoImage = 12,

    [JsonStringEnumMemberName(nameof(PhotoData))]
    PhotoData = 13,

    [JsonStringEnumMemberName(nameof(OtherVideo))]
    OtherVideo = 14,

    [JsonStringEnumMemberName(nameof(OtherVideoData))]
    OtherVideoData = 15,
}
