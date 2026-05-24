using System.Text.Json.Serialization;

namespace Reaparr.AppHost;

/// <summary>
/// The various types of Desktop messages that can be sent
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DesktopMessageType
{
    /// <summary>
    /// None
    /// </summary>
    [JsonStringEnumMemberName(nameof(None))]
    None = 0,

    /// <summary>
    /// For opening an external link
    /// </summary>
    [JsonStringEnumMemberName(nameof(ExternalLink))]
    ExternalLink = 1,

    /// <summary>
    /// Sent by the embedded front-end after the desktop UI has mounted.
    /// </summary>
    [JsonStringEnumMemberName(nameof(DesktopReady))]
    DesktopReady = 2,
}
