namespace Reaparr.Domain;

/// <summary>
/// Represents the type of download client to use for downloading media files.
/// </summary>
public enum PlexDownloadClientType
{
    /// <summary>
    /// Direct download client using HTTP range requests with multi-threaded downloading.
    /// </summary>
    Direct = 0,

    /// <summary>
    /// DASH (MPEG-DASH) download client using dash-mpd-cli for adaptive streaming content.
    /// </summary>
    Dash = 1,
}
