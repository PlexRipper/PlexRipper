namespace Reaparr.Domain;

/// <summary>
/// Represents the type of download client to use for downloading media files.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlexDownloadClientType
{
    /// <summary>
    /// Direct download client using HTTP range requests with multi-threaded downloading.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Direct))]
    Direct = 0,

    /// <summary>
    /// DASH (MPEG-DASH) download client using dash-mpd-cli for adaptive streaming content.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Dash))]
    Dash = 1,
}
