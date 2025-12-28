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
    /// Timestamp of the last metadata update in Plex.
    /// Used to determine whether enrichment or resync is required.
    /// </summary>
    public required DateTime UpdatedAt { get; set; }

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

    /// <summary>
    /// File size in bytes.
    /// Used for Torznab size reporting and change detection.
    /// </summary>
    public required long Size { get; set; }

    /// <summary>
    /// Container format of the file (e.g. mkv, mp4, mpegts).
    /// </summary>
    public required string Container { get; set; }

    /// <summary>
    /// Video profile as reported by Plex (e.g. Advanced, Main 10).
    /// Informational only.
    /// </summary>
    public required string VideoProfile { get; set; }

    /// <summary>
    /// Audio profile as reported by Plex.
    /// Informational only and not used for scoring.
    /// </summary>
    public required string AudioProfile { get; set; }

    /// <summary>
    /// Width of the video stream in pixels.
    /// Used to derive resolution and quality.
    /// </summary>
    public required int Width { get; set; }

    /// <summary>
    /// Height of the video stream in pixels.
    /// Used to derive resolution and quality.
    /// </summary>
    public required int Height { get; set; }

    /// <summary>
    /// Video codec as reported by Plex (e.g. hevc, h264, vc1).
    /// </summary>
    public required string VideoCodec { get; set; }

    /// <summary>
    /// Approximate overall video bitrate.
    /// Used for quality heuristics (e.g. remux detection).
    /// </summary>
    public required int VideoBitrate { get; set; }

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
    /// Derived release source (WEB-DL, BluRay, REMUX).
    /// Determined heuristically from file/container/bitrate.
    /// </summary>
    public required ReleaseSource Source { get; set; }

    /// <summary>
    /// Fully normalized release title used by Sonarr/Radarr.
    /// Typically derived from filename and enriched metadata.
    /// </summary>
    public required string ReleaseTitle { get; set; }

    /// <summary>
    /// Torznab category identifier
    /// (e.g. 2000 Movies, 2040 Movies UHD, 5000 TV).
    /// </summary>
    public required int Category { get; set; }

    /// <summary>
    /// Indicates whether full stream-level metadata
    /// (audio, subtitles, HDR, etc.) has been synced.
    /// </summary>
    public required bool HasMetadata { get; set; }

    /// <summary>
    /// Plex internal index metadata for the part.
    /// Rarely used; informational only.
    /// </summary>
    public string? Indexes { get; set; }

    #endregion

    #region Optional – Filled from SECOND call (/library/metadata/{ratingKey})

    // ---- Video (stream-derived) ----

    /// <summary>
    /// Bit depth of the primary video stream (e.g. 8 or 10).
    /// Required for HDR and quality scoring.
    /// </summary>
    public int BitDepth { get; set; }

    /// <summary>
    /// Indicates presence of any HDR format.
    /// </summary>
    public bool IsHdr { get; set; }

    /// <summary>
    /// Indicates presence of HDR10.
    /// </summary>
    public bool IsHdr10 { get; set; }

    /// <summary>
    /// Indicates presence of Dolby Vision.
    /// </summary>
    public bool IsDolbyVision { get; set; }

    /// <summary>
    /// Color space of the video stream (e.g. bt2020).
    /// </summary>
    public string? ColorSpace { get; set; }

    // ---- Audio (capabilities) ----

    /// <summary>
    /// Best available audio codec for the release
    /// (e.g. TrueHD, DTS-HD MA, EAC3, AC3, AAC).
    /// </summary>
    public required string PrimaryAudioCodec { get; set; }

    /// <summary>
    /// Channel count of the primary audio track.
    /// </summary>
    public required string AudioChannels { get; set; }

    /// <summary>
    /// Maximum number of audio channels available
    /// across all audio tracks.
    /// </summary>
    public int? MaxAudioChannels { get; set; }

    /// <summary>
    /// Indicates whether Dolby Atmos audio is present.
    /// </summary>
    public bool HasAtmos { get; set; }

    /// <summary>
    /// Distinct ISO-639 language codes for all audio tracks,
    /// stored as a comma-separated list.
    /// </summary>
    public string? AudioLanguages { get; set; }

    // ---- Subtitles ----

    /// <summary>
    /// Distinct ISO-639 language codes for all subtitle tracks,
    /// stored as a comma-separated list.
    /// </summary>
    public string? SubtitleLanguages { get; set; }

    /// <summary>
    /// Indicates presence of SDH / hearing-impaired subtitles.
    /// </summary>
    public bool HasSdhSubs { get; set; }

    /// <summary>
    /// Indicates presence of forced subtitles.
    /// </summary>
    public bool HasForcedSubs { get; set; }

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
