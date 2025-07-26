namespace PlexRipper.PlexApi;

public static partial class PlexMediaDataMapper
{
    public static List<PlexMovie> ToPlexMovies(this List<LibraryMediaItemDTO> source) =>
        source.Select(value => value.ToPlexMovie()).ToList();

    public static PlexMovie ToPlexMovie(this LibraryMediaItemDTO source) =>
        new()
        {
            Id = 0,
            Title = source.Title,
            Year = source.Year,

            // This is set later on
            SortIndex = 0,

            SearchTitle = source.Title.ToSearchTitle(),
            Guid = source.Guid,

            Guid_IMDB = source.Guids.GetImdbId(),
            Guid_TMDB = source.Guids.GetTmdbId(),
            Guid_TVDB = source.Guids.GetTvdbId(),

            Duration = source.Duration,
            MediaSize = source.Media.Sum(y => y.Parts.Sum(z => z.Size)),
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,

            Type = PlexMediaType.None,
            Key = int.Parse(source.RatingKey),
            MetaDataKey = RetrieveMetaDataKey(source),
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt.ToDateTime(),
            HasThumb = !string.IsNullOrEmpty(source.Thumb),
            HasArt = !string.IsNullOrEmpty(source.Art),
            HasTheme = !string.IsNullOrEmpty(source.Theme),

            Countries = source.Country.ToPlexCountry(),
            Actors = source.Role.ToPlexActor(),
            Genres = source.Genre.ToPlexGenre(),
            MediaDataList = source.Media.ToMovieMediaDataList(),

            // Ignore the following
            FullTitle = string.Empty,
            PlexLibraryId = 0,
            PlexServerId = 0,
            FullBannerUrl = string.Empty,
        };

    public static ICollection<PlexMovieMediaData> ToMovieMediaDataList(this List<LibraryMediaItemMediaDTO> source) =>
        source.Select(x => x.ToMovieMediaDataList()).ToList();

    public static PlexMovieMediaData ToMovieMediaDataList(this LibraryMediaItemMediaDTO source) =>
        new()
        {
            Id = 0,
            PlexId = source.Id,
            Duration = source.Duration,
            Bitrate = source.Bitrate,
            Width = source.Width,
            Height = source.Height,
            AspectRatio = source.AspectRatio,
            AudioChannels = source.AudioChannels,
            AudioCodec = source.AudioCodec,
            VideoCodec = source.VideoCodec,
            RawVideoResolution = source.VideoResolution,
            Quality = source.VideoResolution.ToVideoQuality(),
            Container = source.Container,
            VideoFrameRate = source.VideoFrameRate,
            VideoProfile = source.VideoProfile,
            AudioProfile = source.AudioProfile,
            HasVoiceActivity = source.HasVoiceActivity,
            Parts = source.Parts.ToPlexMovieModel(),

            // Ignore the following
            PlexLibraryId = 0,
            PlexServerId = 0,
            PlexMovieId = 0,
        };

    public static ICollection<PlexMovieMediaDataPart> ToPlexMovieModel(this List<LibraryMediaItemPartDTO> source) =>
        source.Select(x => x.ToPlexMovieModel()).ToList();

    public static PlexMovieMediaDataPart ToPlexMovieModel(this LibraryMediaItemPartDTO source) =>
        new()
        {
            Id = 0,
            PlexId = source.Id,
            Accessible = source.Accessible,
            Exists = source.Exists,
            Key = source.Key,
            Indexes = source.Indexes,
            Duration = source.Duration,
            File = source.File,
            Size = source.Size,
            Container = source.Container,
            VideoProfile = source.VideoProfile,
            AudioProfile = source.AudioProfile,
            Streams = source.Stream.ToPlexMovieModel(),

            // Ignore the following
            PlexLibraryId = 0,
            PlexServerId = 0,
            PlexMovieId = 0,
            PlexMovieMediaDataId = 0,
        };

    public static ICollection<PlexMovieMediaDataStream> ToPlexMovieModel(this List<LibraryMediaItemStreamDTO> source) =>
        source.Select(x => x.ToPlexMovieModel()).ToList();

    public static PlexMovieMediaDataStream ToPlexMovieModel(this LibraryMediaItemStreamDTO source) =>
        new()
        {
            Id = 0,
            PlexId = source.Id,
            StreamType = source.StreamType,
            Default = source.Default,
            Codec = source.Codec,
            Index = source.Index,
            Bitrate = source.Bitrate,
            Language = source.Language,
            LanguageTag = source.LanguageTag,
            LanguageCode = source.LanguageCode,
            DOVIBLCompatID = source.DOVIBLCompatID,
            DOVIBLPresent = source.DOVIBLPresent,
            DOVIELPresent = source.DOVIELPresent,
            DOVILevel = source.DOVILevel,
            DOVIPresent = source.DOVIPresent,
            DOVIProfile = source.DOVIProfile,
            DOVIRPUPresent = source.DOVIRPUPresent,
            DOVIVersion = source.DOVIVersion,
            BitDepth = source.BitDepth,
            ChromaLocation = source.ChromaLocation,
            ChromaSubsampling = source.ChromaSubsampling,
            CodedHeight = source.CodedHeight,
            CodedWidth = source.CodedWidth,
            ColorPrimaries = source.ColorPrimaries,
            ColorRange = source.ColorRange,
            ColorSpace = source.ColorSpace,
            ColorTrc = source.ColorTrc,
            FrameRate = source.FrameRate,
            Height = source.Height,
            Level = source.Level,
            Original = source.Original,
            HasScalingMatrix = source.HasScalingMatrix,
            Profile = source.Profile,
            ScanType = source.ScanType,
            RefFrames = source.RefFrames,
            Width = source.Width,
            DisplayTitle = source.DisplayTitle,
            ExtendedDisplayTitle = source.ExtendedDisplayTitle,
            Selected = source.Selected,
            Forced = source.Forced,
            Channels = source.Channels,
            AudioChannelLayout = source.AudioChannelLayout,
            SamplingRate = source.SamplingRate,
            CanAutoSync = source.CanAutoSync,
            HearingImpaired = source.HearingImpaired,
            Dub = source.Dub,
            Title = source.Title,

            // Ignore the following
            PlexLibraryId = 0,
            PlexServerId = 0,
            PlexMovieId = 0,
            PlexMovieMediaDataId = 0,
            PlexMovieMediaDataPartId = 0,
        };
}
