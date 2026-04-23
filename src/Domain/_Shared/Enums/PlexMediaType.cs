namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlexMediaType
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc. and that there is no skip in between
    // Otherwise the TypeScript DTO translator in the front-end starts messing up
    [JsonStringEnumMemberName(nameof(None))]
    None = 0,

    [JsonStringEnumMemberName(nameof(Movie))]
    Movie = 1,

    [JsonStringEnumMemberName(nameof(TvShow))]
    TvShow = 2,

    [JsonStringEnumMemberName(nameof(Season))]
    Season = 3,

    [JsonStringEnumMemberName(nameof(Episode))]
    Episode = 4,

    [JsonStringEnumMemberName(nameof(Music))]
    Music = 5,

    [JsonStringEnumMemberName(nameof(Artist))]
    Artist = 6,

    [JsonStringEnumMemberName(nameof(Album))]
    Album = 7,

    // TODO rename to track
    [JsonStringEnumMemberName(nameof(Song))]
    Song = 8,

    [JsonStringEnumMemberName(nameof(PhotoAlbum))]
    PhotoAlbum = 9,

    [JsonStringEnumMemberName(nameof(Photos))]
    Photos = 10,

    [JsonStringEnumMemberName(nameof(OtherVideos))]
    OtherVideos = 11,

    [JsonStringEnumMemberName(nameof(Games))]
    Games = 12,

    [JsonStringEnumMemberName(nameof(Unknown))]
    Unknown = 13,
}
