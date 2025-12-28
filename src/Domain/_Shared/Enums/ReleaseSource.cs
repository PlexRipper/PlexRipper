using System.Runtime.Serialization;
using System.Text.Json.Serialization;

// ReSharper disable InconsistentNaming

namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReleaseSource
{
    [EnumMember(Value = "")]
    None = 0,

    [EnumMember(Value = "Remux")]
    Remux = 1,

    [EnumMember(Value = "BluRay")]
    BluRay = 2,

    [EnumMember(Value = "WEB-DL")]
    WebDl = 3,

    [EnumMember(Value = "WEBRip")]
    WebRip = 4,

    [EnumMember(Value = "DVD")]
    DVD = 5,

    [EnumMember(Value = "HDTV")]
    HDTV = 6,
}
