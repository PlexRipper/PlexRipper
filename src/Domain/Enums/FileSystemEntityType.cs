using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PlexRipper.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FileSystemEntityType
    {
        [EnumMember(Value = "Parent")]
        Parent = 0,

        [EnumMember(Value = "Drive")]
        Drive = 1,

        [EnumMember(Value = "Folder")]
        Folder = 2,

        [EnumMember(Value = "File")]
        File = 3,
    }
}