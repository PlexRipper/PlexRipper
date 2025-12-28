namespace Reaparr.Domain;

public abstract class BasePlexMediaDataPart : BaseEntity
{
    #region Required – Filled from FIRST call (/library/sections/.../all)

    /// <summary>
    /// Plex Part.id.
    /// Immutable identifier for the physical media file.
    /// Used as the primary file identity for incremental sync.
    /// </summary>
    public required long PlexId { get; set; }

    /// <summary>
    /// Plex Metadata.ratingKey.
    /// Identifies the logical media item (movie / episode) this file belongs to.
    /// <example>"23920"</example>
    /// </summary>
    public required int RatingKey { get; set; }

    /// <summary>
    /// Plex-generated key used to access this specific part.
    /// <example>"/library/parts/47140/1712273142/file.mkv"</example>
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Duration of the media file in milliseconds.
    /// </summary>
    public required int Duration { get; set; }

    /// <summary>
    /// Absolute filesystem path to the media file.
    /// </summary>
    public required string File { get; set; }

    public required string OriginalFilename { get; set; }

    public required string GeneratedFilename { get; set; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public required long Size { get; set; }

    /// <summary>
    /// Container format of the file (e.g. mkv, mp4, mpegts).
    /// </summary>
    public required string Container { get; set; }

    /// <summary>
    /// Indicates whether full stream-level metadata
    /// (audio, subtitles, HDR, etc.) has been synced.
    /// </summary>
    public required bool HasMetadata { get; set; }

    /// <summary>
    /// Timestamp of the last metadata update in Plex.
    /// Used to determine whether enrichment or resync is required.
    /// </summary>
    public required DateTime LastSyncedAt { get; set; }

    #region Metadata Sync Derived

    /// <summary>
    /// Video codec as reported by Plex (e.g. hevc, h264, vc1).
    /// </summary>
    public required string VideoCodec { get; set; }

    /// <summary>
    /// Frame rate of the video stream.
    /// </summary>
    public required decimal FrameRate { get; set; }

    /// <summary>
    /// Normalized resolution label derived from video height
    /// (480p / 720p / 1080p / 2160p).
    /// </summary>
    public required string Resolution { get; set; }

    /// <summary>
    /// Derived release source (WEB-DL, Blu-ray, REMUX).
    /// Determined heuristically from file/container/bitrate.
    /// </summary>
    public required ReleaseSource Source { get; set; }

    /// <summary>
    /// Best available audio codec for the release
    /// (e.g. TrueHD, DTS-HD MA, EAC3, AC3, AAC).
    /// </summary>
    public required string PrimaryAudioCodec { get; set; }

    /// <summary>
    /// Channel count of the primary audio track.
    /// </summary>
    public required string AudioChannels { get; set; }

    #endregion

    #endregion


    #region Relationships

    /// <summary>
    /// Identifier of the Plex library section this part belongs to.
    /// </summary>
    public required int PlexLibraryId { get; set; }

    /// <summary>
    /// Identifier of the Plex server this part was synced from.
    /// </summary>
    public required int PlexServerId { get; set; }

    /// <summary>
    /// Navigation property to the Plex library.
    /// </summary>
    public PlexLibrary? PlexLibrary { get; set; }

    /// <summary>
    /// Navigation property to the Plex server.
    /// </summary>
    public PlexServer? PlexServer { get; init; }

    #endregion
}
