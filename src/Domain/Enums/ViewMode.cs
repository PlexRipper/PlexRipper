using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PlexRipper.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ViewMode
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc and that there is no skip in between
    // Otherwise the Typescript DTO translator in the front-end starts messing up
    [EnumMember(Value = "Poster")]
    Poster = 0,

    [EnumMember(Value = "Table")]
    Table = 1,
}
