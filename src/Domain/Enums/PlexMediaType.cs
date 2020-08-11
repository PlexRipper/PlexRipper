using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PlexRipper.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlexMediaType
    {
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

        [EnumMember(Value = "Unknown")]
        Unknown = 10
    }
}
