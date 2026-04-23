namespace Reaparr.Domain;

/// <summary>
/// Represents normalized video resolution tiers used by Plex.
/// </summary>
// ReSharper disable InconsistentNaming
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VideoQuality
{
    /// <summary>
    /// The "null" quality, indicating no specific quality level.
    /// </summary>
    [JsonStringEnumMemberName(nameof(None))]
    None = -1,

    /// <summary>
    /// The resolution could not be determined or was not recognized.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Unknown))]
    Unknown = 0,

    /// <summary>
    /// Extremely low resolution, typically 144p (256×144). Used for very low-bandwidth scenarios.
    /// </summary>
    [JsonStringEnumMemberName(nameof(SubSD_144p))]
    SubSD_144p = 144,

    /// <summary>
    /// Sub-SD resolution, typically 240p (352×240). Used in very low-quality streaming.
    /// </summary>
    [JsonStringEnumMemberName(nameof(SubSD_CIF))]
    SubSD_CIF = 240,

    /// <summary>
    /// nHD resolution, typically 360p (640×360). Common for low-bandwidth streaming.
    /// </summary>
    [JsonStringEnumMemberName(nameof(nHD))]
    nHD = 360,

    /// <summary>
    /// Standard Definition, typically 480p.
    /// </summary>
    [JsonStringEnumMemberName(nameof(SD))]
    SD = 480,

    /// <summary>
    /// PAL Standard Definition (576p).
    /// </summary>
    [JsonStringEnumMemberName(nameof(DVD))]
    DVD = 576,

    /// <summary>
    /// High Definition (720p).
    /// </summary>
    [JsonStringEnumMemberName(nameof(HD))]
    HD = 720,

    /// <summary>
    /// Full High Definition (1080p).
    /// </summary>
    [JsonStringEnumMemberName(nameof(FullHD))]
    FullHD = 1080,

    /// <summary>
    /// Quad HD (1440p).
    /// </summary>
    [JsonStringEnumMemberName(nameof(QHD))]
    QHD = 1440,

    /// <summary>
    /// Ultra HD or 4K (2160p).
    /// </summary>
    [JsonStringEnumMemberName(nameof(UHD_4K))]
    UHD_4K = 2160,

    /// <summary>
    /// Full Ultra HD or 8K (4320p).
    /// </summary>
    [JsonStringEnumMemberName(nameof(UHD_8K))]
    UHD_8K = 4320,
}
