using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Reaparr.Application;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TestConnectionStatus
{
    [EnumMember(Value = nameof(Unknown))]
    Unknown = 0,

    [EnumMember(Value = nameof(Success))]
    Success = 1,

    [EnumMember(Value = nameof(UrlIsInvalid))]
    UrlIsInvalid = 2,

    [EnumMember(Value = nameof(ConnectionFailed))]
    ConnectionFailed = 3,

    [EnumMember(Value = nameof(InvalidApiKey))]
    InvalidApiKey = 4,
}
