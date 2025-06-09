using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PlexRipper.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlexMediaType
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc and that there is no skip in between
    // Otherwise the Typescript DTO translator in the front-end starts messing up
    [EnumMember(Value = nameof(None))]
    None = 0,

    [EnumMember(Value = nameof(Movie))]
    Movie = 1,

    [EnumMember(Value = nameof(TvShow))]
    TvShow = 2,

    [EnumMember(Value = nameof(Season))]
    Season = 3,

    [EnumMember(Value = nameof(Episode))]
    Episode = 4,

    [EnumMember(Value = nameof(Music))]
    Music = 5,

    [EnumMember(Value = nameof(Artist))]
    Artist = 6,

    [EnumMember(Value = nameof(Album))]
    Album = 7,

    [EnumMember(Value = nameof(Song))]
    Song = 8,

    [EnumMember(Value = nameof(PhotoAlbum))]
    PhotoAlbum = 9,

    [EnumMember(Value = nameof(Photos))]
    Photos = 10,

    [EnumMember(Value = nameof(OtherVideos))]
    OtherVideos = 11,

    [EnumMember(Value = nameof(Games))]
    Games = 12,

    [EnumMember(Value = nameof(Unknown))]
    Unknown = 13,
}
