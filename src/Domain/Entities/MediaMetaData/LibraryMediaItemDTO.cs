namespace PlexRipper.Domain;

public record MediaDataContainer(List<LibraryMediaItemMediaDTO> MediaData)
{
    public List<LibraryMediaItemMediaDTO> MediaData { get; set; } = MediaData;
}

public record LibraryMediaItemDTO
{
    public required string RatingKey { get; set; }

    public required string Key { get; set; }

    public required PlexMediaType Type { get; set; }

    public required string Title { get; set; }

    public required string Summary { get; set; }

    public required int Year { get; set; }

    public required string Studio { get; set; }

    public required string ContentRating { get; set; }

    public required string TitleSort { get; set; }

    public required string OriginalTitle { get; set; }

    public required int ChildCount { get; set; }

    public required int Duration { get; set; }

    public required float Rating { get; set; }

    public required string Thumb { get; set; }

    public required string Art { get; set; }

    public required string Theme { get; set; }

    public required string Guid { get; set; }

    public required string GrandparentTitle { get; set; }

    public required string ParentTitle { get; set; }

    public required string ParentGuid { get; set; }

    public required string ParentRatingKey { get; set; }

    public required double? AudienceRating { get; set; }

    public required DateTime AddedAt { get; set; }

    public required DateTime UpdatedAt { get; set; }

    public required string OriginallyAvailableAt { get; set; }

    public required List<MetaDataRatingsDTO> Ratings { get; set; } = [];

    public required List<MetaDataGuidsDTO> Guids { get; set; } = [];

    public required List<LibraryMediaItemMediaDTO> Media { get; set; } = [];

    public required List<LibraryMediaItemGenreDTO> Genre { get; set; } = [];

    public required List<LibraryMediaItemCountryDTO> Country { get; set; } = [];

    public required List<LibraryMediaItemRoleDTO> Role { get; set; } = [];
}

public record MetaDataRatingsDTO
{
    /// <summary>
    /// The image or reference for the rating.
    /// </summary>
    public required string Image { get; set; }

    /// <summary>
    /// The rating value.
    /// </summary>
    public required float Value { get; set; }

    /// <summary>
    /// The type of rating (e.g., audience, critic).
    /// </summary>
    public required string Type { get; set; }
}

public record MetaDataGuidsDTO
{
    /// <summary>
    /// The GUID value.
    /// </summary>
    public required string Id { get; set; }
}

public record LibraryMediaItemCountryDTO
{
    public required int PlexId { get; init; }

    public required string Name { get; init; }

    public required string Filter { get; init; }

    /// <summary>
    /// A MD5 hash of the name, used as a unique key.
    /// </summary>
    public required string Key { get; init; }
}

public record LibraryMediaItemGenreDTO
{
    public required int PlexId { get; init; }

    public required string Name { get; init; }

    public required string Filter { get; init; }

    /// <summary>
    /// A MD5 hash of the name, used as a unique key.
    /// </summary>
    public required string Key { get; init; }
}

public record LibraryMediaItemRoleDTO
{
    public required int PlexId { get; init; }

    public required string Name { get; init; }

    public required string? Role { get; init; }

    public required string? Filter { get; init; }

    public required string? Thumb { get; init; }

    /// <summary>
    /// A MD5 hash of the name, used as a unique key.
    /// </summary>
    public required string Key { get; init; }
}

public record LibraryMediaItemMediaDTO
{
    /// <summary>
    /// Unique media identifier.
    /// </summary>
    public required long Id { get; set; }

    /// <summary>
    /// Duration of the media in milliseconds.
    /// </summary>
    public required int Duration { get; set; }

    /// <summary>
    /// Bitrate in bits per second.
    /// </summary>
    public required int Bitrate { get; set; }

    /// <summary>
    /// Video width in pixels.
    /// </summary>
    public required int Width { get; set; }

    /// <summary>
    /// Video height in pixels.
    /// </summary>
    public required int Height { get; set; }

    /// <summary>
    /// Aspect ratio of the video.
    /// </summary>
    public required float AspectRatio { get; set; }

    /// <summary>
    /// Number of audio channels.
    /// </summary>
    public required int AudioChannels { get; set; }

    /// <summary>
    /// Audio codec used.
    /// </summary>
    public required string AudioCodec { get; set; }

    /// <summary>
    /// Video codec used.
    /// </summary>
    public required string VideoCodec { get; set; }

    /// <summary>
    /// Video resolution (e.g., 4k).
    /// </summary>
    public required string VideoResolution { get; set; }

    /// <summary>
    /// File container type.
    /// </summary>
    public required string Container { get; set; }

    /// <summary>
    /// Frame rate of the video (e.g., 24p).
    /// </summary>
    public required string VideoFrameRate { get; set; }

    /// <summary>
    /// Video profile (e.g., main 10).
    /// </summary>
    public required string VideoProfile { get; set; }

    /// <summary>
    /// Video profile (e.g., main 10).
    /// </summary>
    public required string AudioProfile { get; set; }

    /// <summary>
    /// Indicates whether voice activity is detected.
    /// </summary>
    public required bool HasVoiceActivity { get; set; }

    /// <summary>
    /// An array of parts for this media item.
    /// </summary>
    public required List<LibraryMediaItemPartDTO> Parts { get; set; }
}

public record LibraryMediaItemPartDTO
{
    /// <summary>
    /// Indicates if the part is accessible.
    /// </summary>
    public required bool? Accessible { get; set; }

    /// <summary>
    /// Indicates if the part exists.
    /// </summary>
    public required bool? Exists { get; set; }

    /// <summary>
    /// Unique part identifier.
    /// </summary>
    public required long Id { get; set; }

    /// <summary>
    /// Key to access this part.
    /// </summary>
    public required string Key { get; set; }

    public required string? Indexes { get; set; }

    /// <summary>
    /// Duration of the part in milliseconds.
    /// </summary>
    public required int Duration { get; set; }

    /// <summary>
    /// File path for the part.
    /// </summary>
    public required string File { get; set; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public required long Size { get; set; }

    /// <summary>
    /// Container format of the part.
    /// </summary>
    public required string Container { get; set; }

    /// <summary>
    /// Video profile for the part.
    /// </summary>
    public required string VideoProfile { get; set; }

    public required string AudioProfile { get; set; }

    /// <summary>
    /// An array of streams for this part.
    /// </summary>
    public required List<LibraryMediaItemStreamDTO> Stream { get; set; }
}

public record LibraryMediaItemStreamDTO
{
    /// <summary>
    /// Unique stream identifier.
    /// </summary>
    public required long Id { get; set; }

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
}
