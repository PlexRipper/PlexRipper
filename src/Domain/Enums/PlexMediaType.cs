using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PlexRipper.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlexMediaType
    {
        // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc and that there is no skip in between
        // Otherwise the Typescript DTO translator in the front-end starts messing up

        [EnumMember(Value = "None")]
        None = 0,

        [EnumMember(Value = "Movie")]
        Movie = 1,

        [EnumMember(Value = "TvShow")]
        TvShow = 2,

        [EnumMember(Value = "Season")]
        Season = 3,

        [EnumMember(Value = "Episode")]
        Episode = 4,

        [EnumMember(Value = "Music")]
        Music = 5,

        [EnumMember(Value = "Album")]
        Album = 6,

        [EnumMember(Value = "Song")]
        Song = 7,

        [EnumMember(Value = "Unknown")]
        Unknown = 10,
    }
}