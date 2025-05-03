namespace PlexRipper.Domain;

public abstract class BasePlexMediaDataStream : BaseEntity
{
    /// <summary>
    /// Unique stream identifier.
    /// </summary>
    public required long PlexId { get; set; }

    /// <summary>
    /// Stream type (1=video, 2=audio, 3=subtitle).
    /// </summary>
    public required StreamType StreamType { get; set; }

    /// <summary>
    /// Indicates if this stream is default.
    /// </summary>
    public required bool? Default { get; set; }

    /// <summary>
    /// Codec used by the stream.
    /// </summary>
    public required string Codec { get; set; }

    /// <summary>
    /// Index of the stream.
    /// </summary>
    public required int? Index { get; set; }

    /// <summary>
    /// Bitrate of the stream.
    /// </summary>
    public required int Bitrate { get; set; }

    /// <summary>
    /// Language of the stream.
    /// </summary>
    public required string Language { get; set; }

    /// <summary>
    /// Language tag (e.g., en).
    /// </summary>
    public required string LanguageTag { get; set; }

    /// <summary>
    /// ISO language code.
    /// </summary>
    public required string LanguageCode { get; set; }

    /// <summary>
    /// Dolby Vision BL compatibility ID.
    /// </summary>
    public required int? DOVIBLCompatID { get; set; }

    /// <summary>
    /// Indicates if Dolby Vision BL is present.
    /// </summary>
    public required bool? DOVIBLPresent { get; set; }

    /// <summary>
    /// Indicates if Dolby Vision EL is present.
    /// </summary>
    public required bool? DOVIELPresent { get; set; }

    /// <summary>
    /// Dolby Vision level.
    /// </summary>
    public required int? DOVILevel { get; set; }

    /// <summary>
    /// Indicates if Dolby Vision is present.
    /// </summary>
    public required bool? DOVIPresent { get; set; }

    /// <summary>
    /// Dolby Vision profile.
    /// </summary>
    public required int? DOVIProfile { get; set; }

    /// <summary>
    /// Indicates if Dolby Vision RPU is present.
    /// </summary>
    public required bool? DOVIRPUPresent { get; set; }

    /// <summary>
    /// Dolby Vision version.
    /// </summary>
    public required string? DOVIVersion { get; set; }

    /// <summary>
    /// Bit depth of the video stream.
    /// </summary>
    public required int? BitDepth { get; set; }

    /// <summary>
    /// Chroma sample location.
    /// </summary>
    public required string? ChromaLocation { get; set; }

    /// <summary>
    /// Chroma subsampling format.
    /// </summary>
    public required string? ChromaSubsampling { get; set; }

    /// <summary>
    /// Coded video height.
    /// </summary>
    public required int? CodedHeight { get; set; }

    /// <summary>
    /// Coded video width.
    /// </summary>
    public required int? CodedWidth { get; set; }

    /// <summary>
    /// Color primaries used.
    /// </summary>
    public required string? ColorPrimaries { get; set; }

    /// <summary>
    /// Color range (e.g., tv).
    /// </summary>
    public required string? ColorRange { get; set; }

    /// <summary>
    /// Color space.
    /// </summary>
    public required string? ColorSpace { get; set; }

    /// <summary>
    /// Color transfer characteristics.
    /// </summary>
    public required string? ColorTrc { get; set; }

    /// <summary>
    /// Frame rate of the stream.
    /// </summary>
    public required float? FrameRate { get; set; }

    /// <summary>
    /// Height of the video stream.
    /// </summary>
    public required int? Height { get; set; }

    /// <summary>
    /// Video level.
    /// </summary>
    public required int? Level { get; set; }

    /// <summary>
    /// Indicates if this is the original stream.
    /// </summary>
    public required bool? Original { get; set; }

    /// <summary>
    /// Indicates if a scaling matrix is present.
    /// </summary>
    public required bool? HasScalingMatrix { get; set; }

    /// <summary>
    /// Video profile.
    /// </summary>
    public required string? Profile { get; set; }

    /// <summary>
    /// Scan type.
    /// </summary>
    public required string? ScanType { get; set; }

    /// <summary>
    /// Number of reference frames.
    /// </summary>
    public required int? RefFrames { get; set; }

    /// <summary>
    /// Width of the video stream.
    /// </summary>
    public required int? Width { get; set; }

    /// <summary>
    /// Display title for the stream.
    /// </summary>
    public required string DisplayTitle { get; set; }

    /// <summary>
    /// Extended display title for the stream.
    /// </summary>
    public required string ExtendedDisplayTitle { get; set; }

    /// <summary>
    /// Indicates if this stream is selected (applicable for audio streams).
    /// </summary>
    public required bool? Selected { get; set; }

    /// <summary>
    /// Indicates if this stream is forced.
    /// </summary>
    public required bool? Forced { get; set; }

    /// <summary>
    /// Number of audio channels (for audio streams).
    /// </summary>
    public required int? Channels { get; set; }

    /// <summary>
    /// Audio channel layout.
    /// </summary>
    public required string? AudioChannelLayout { get; set; }

    /// <summary>
    /// Sampling rate for the audio stream.
    /// </summary>
    public required int? SamplingRate { get; set; }

    /// <summary>
    /// Indicates if the stream can auto-sync.
    /// </summary>
    public required bool? CanAutoSync { get; set; }

    /// <summary>
    /// Indicates if the stream is for the hearing impaired.
    /// </summary>
    public required bool? HearingImpaired { get; set; }

    /// <summary>
    /// Indicates if the stream is a dub.
    /// </summary>
    public required bool? Dub { get; set; }

    /// <summary>
    /// Optional title for the stream (e.g., language variant).
    /// </summary>
    public required string? Title { get; set; }

    #region Relationships

    public required int PlexLibraryId { get; set; }

    public required int PlexServerId { get; set; }

    public PlexLibrary? PlexLibrary { get; set; }

    public PlexServer? PlexServer { get; init; }

    #endregion
}
