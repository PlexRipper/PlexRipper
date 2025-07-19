using System.Runtime.Serialization;

/// <summary>
/// Represents normalized video resolution tiers used by Plex.
/// </summary>
public enum VideoQuality
{
    /// <summary>
    /// The resolution could not be determined or was not recognized.
    /// </summary>
    [EnumMember(Value = "unknown")]
    Unknown = 0,

    /// <summary>
    /// Standard Definition, typically 480p.
    /// </summary>
    [EnumMember(Value = "480p")]
    SD = 480,

    /// <summary>
    /// PAL Standard Definition (576p).
    /// </summary>
    [EnumMember(Value = "576p")]
    DVD = 576,

    /// <summary>
    /// High Definition (720p).
    /// </summary>
    [EnumMember(Value = "720p")]
    HD = 720,

    /// <summary>
    /// Full High Definition (1080p).
    /// </summary>
    [EnumMember(Value = "1080p")]
    FullHD = 1080,

    /// <summary>
    /// Quad HD (1440p).
    /// </summary>
    [EnumMember(Value = "1440p")]
    QHD = 1440,

    /// <summary>
    /// Ultra HD or 4K (2160p).
    /// </summary>
    [EnumMember(Value = "4K")]
    UHD_4K = 2160,

    /// <summary>
    /// Full Ultra HD or 8K (4320p).
    /// </summary>
    [EnumMember(Value = "8K")]
    UHD_8K = 4320,
}
