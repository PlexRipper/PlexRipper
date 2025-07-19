using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Represents normalized video resolution tiers used by Plex.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VideoQuality
{
    /// <summary>
    /// The resolution could not be determined or was not recognized.
    /// </summary>
    [EnumMember(Value = nameof(Unknown))]
    Unknown = 0,

    /// <summary>
    /// Standard Definition, typically 480p.
    /// </summary>
    [EnumMember(Value = nameof(SD))]
    SD = 480,

    /// <summary>
    /// PAL Standard Definition (576p).
    /// </summary>
    [EnumMember(Value = nameof(DVD))]
    DVD = 576,

    /// <summary>
    /// High Definition (720p).
    /// </summary>
    [EnumMember(Value = nameof(HD))]
    HD = 720,

    /// <summary>
    /// Full High Definition (1080p).
    /// </summary>
    [EnumMember(Value = nameof(FullHD))]
    FullHD = 1080,

    /// <summary>
    /// Quad HD (1440p).
    /// </summary>
    [EnumMember(Value = nameof(QHD))]
    QHD = 1440,

    /// <summary>
    /// Ultra HD or 4K (2160p).
    /// </summary>
    [EnumMember(Value = nameof(UHD_4K))]
    UHD_4K = 2160,

    /// <summary>
    /// Full Ultra HD or 8K (4320p).
    /// </summary>
    [EnumMember(Value = nameof(UHD_8K))]
    UHD_8K = 4320,
}
