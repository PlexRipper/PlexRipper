using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PlexRipper.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NotificationLevel
    {
        [EnumMember(Value = "none")]
        None = 0,

        [EnumMember(Value = "info")]
        Info = 1,

        [EnumMember(Value = "success")]
        Success = 2,

        [EnumMember(Value = "warning")]
        Warning = 3,

        [EnumMember(Value = "error")]
        Error = 4,
    }
}