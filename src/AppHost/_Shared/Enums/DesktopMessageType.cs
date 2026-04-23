using System.Runtime.Serialization;
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
    [EnumMember(Value = nameof(None))]
    None = 0,

    /// <summary>
    /// For opening an external link
    /// </summary>
    [EnumMember(Value = nameof(ExternalLink))]
    ExternalLink = 1,
}
