using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PlexRipper.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ViewMode
    {
        [EnumMember(Value = "Table")]
        Table = 0,

        [EnumMember(Value = "Poster")]
        Poster = 1,

        [EnumMember(Value = "Overview")]
        Overview = 2,
    }
}