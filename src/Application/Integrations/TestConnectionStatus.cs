namespace Reaparr.Application;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TestConnectionStatus
{
    [JsonStringEnumMemberName(nameof(Unknown))]
    Unknown = 0,

    [JsonStringEnumMemberName(nameof(Success))]
    Success = 1,

    [JsonStringEnumMemberName(nameof(UrlIsInvalid))]
    UrlIsInvalid = 2,

    [JsonStringEnumMemberName(nameof(ConnectionFailed))]
    ConnectionFailed = 3,

    [JsonStringEnumMemberName(nameof(InvalidApiKey))]
    InvalidApiKey = 4,
}
