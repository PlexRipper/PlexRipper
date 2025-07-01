namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    private static Faker<T> ApplyBasePlexMediaDataStream<T>(this Faker<T> faker)
        where T : BasePlexMediaDataStream =>
        faker
            .StrictMode(true)
            .Ignore(x => x.Id)
            .RuleFor(x => x.PlexId, f => f.Random.Long(1))
            .RuleFor(x => x.StreamType, f => f.PickRandom<StreamType>())
            .RuleFor(x => x.Default, f => f.Random.Bool())
            .RuleFor(x => x.Codec, f => f.Random.AlphaNumeric(4))
            .RuleFor(x => x.Index, f => f.Random.Int(0, 3))
            .RuleFor(x => x.Bitrate, f => f.Random.Int(100_000, 1_000_000))
            .RuleFor(x => x.Language, f => f.PickRandom("English", "Spanish", "French"))
            .RuleFor(x => x.LanguageTag, f => f.PickRandom("en", "es", "fr"))
            .RuleFor(x => x.LanguageCode, f => f.PickRandom("eng", "spa", "fre"))
            .RuleFor(x => x.DOVIBLCompatID, f => f.Random.Int(1, 5))
            .RuleFor(x => x.DOVIBLPresent, f => f.Random.Bool())
            .RuleFor(x => x.DOVIELPresent, f => f.Random.Bool())
            .RuleFor(x => x.DOVILevel, f => f.Random.Int(1, 10))
            .RuleFor(x => x.DOVIPresent, f => f.Random.Bool())
            .RuleFor(x => x.DOVIProfile, f => f.Random.Int(1, 10))
            .RuleFor(x => x.DOVIRPUPresent, f => f.Random.Bool())
            .RuleFor(x => x.DOVIVersion, f => f.Random.Float(1, 2).ToString("0.0"))
            .RuleFor(x => x.BitDepth, f => f.PickRandom(8, 10, 12))
            .RuleFor(x => x.ChromaLocation, f => f.PickRandom("left", "center"))
            .RuleFor(x => x.ChromaSubsampling, f => f.PickRandom("4:2:0", "4:2:2"))
            .RuleFor(x => x.CodedHeight, f => f.Random.Int(720, 1080))
            .RuleFor(x => x.CodedWidth, f => f.Random.Int(1280, 1920))
            .RuleFor(x => x.ColorPrimaries, f => f.PickRandom("bt709", "smpte170m"))
            .RuleFor(x => x.ColorRange, f => f.PickRandom("tv", "pc"))
            .RuleFor(x => x.ColorSpace, f => f.PickRandom("bt709", "bt2020"))
            .RuleFor(x => x.ColorTrc, f => f.PickRandom("bt709", "smpte170m"))
            .RuleFor(x => x.FrameRate, f => f.Random.Float(23.976f, 60f))
            .RuleFor(x => x.Height, f => f.Random.Int(720, 1080))
            .RuleFor(x => x.Level, f => f.Random.Int(1, 5))
            .RuleFor(x => x.Original, f => f.Random.Bool())
            .RuleFor(x => x.HasScalingMatrix, f => f.Random.Bool())
            .RuleFor(x => x.Profile, f => f.Random.Word())
            .RuleFor(x => x.ScanType, f => f.PickRandom("progressive", "interlaced"))
            .RuleFor(x => x.RefFrames, f => f.Random.Int(1, 5))
            .RuleFor(x => x.Width, f => f.Random.Int(1280, 1920))
            .RuleFor(x => x.DisplayTitle, f => f.Lorem.Sentence(3))
            .RuleFor(x => x.ExtendedDisplayTitle, f => f.Lorem.Sentence(5))
            .RuleFor(x => x.Selected, f => f.Random.Bool())
            .RuleFor(x => x.Forced, f => f.Random.Bool())
            .RuleFor(x => x.Channels, f => f.PickRandom(2, 6, 8))
            .RuleFor(x => x.AudioChannelLayout, f => f.PickRandom("stereo", "5.1", "7.1"))
            .RuleFor(x => x.SamplingRate, f => f.Random.Int(44100, 96000))
            .RuleFor(x => x.CanAutoSync, f => f.Random.Bool())
            .RuleFor(x => x.HearingImpaired, f => f.Random.Bool())
            .RuleFor(x => x.Dub, f => f.Random.Bool())
            .RuleFor(x => x.Title, f => f.Lorem.Word())
            .Ignore(x => x.PlexServerId)
            .Ignore(x => x.PlexServer)
            .Ignore(x => x.PlexLibraryId)
            .Ignore(x => x.PlexLibrary);

    private static Faker<PlexMovieMediaDataStream> _plexMovieMediaDataStream = new Faker<PlexMovieMediaDataStream>()
        .ApplyBasePlexMediaDataStream()
        .Ignore(x => x.PlexMovieId)
        .Ignore(x => x.PlexMovie)
        .Ignore(x => x.PlexMovieMediaDataId)
        .Ignore(x => x.PlexMovieMediaData)
        .Ignore(x => x.PlexMovieMediaDataPartId)
        .Ignore(x => x.PlexMovieMediaDataPart);

    private static Faker<PlexTvShowEpisodeMediaDataStream> _plexTvShowEpisodeMediaDataStream =
        new Faker<PlexTvShowEpisodeMediaDataStream>()
            .ApplyBasePlexMediaDataStream()
            .Ignore(x => x.PlexTvShowEpisodeId)
            .Ignore(x => x.PlexTvShowEpisode)
            .Ignore(x => x.PlexTvShowEpisodeMediaDataId)
            .Ignore(x => x.PlexTvShowEpisodeMediaData)
            .Ignore(x => x.PlexTvShowEpisodeMediaDataPartId)
            .Ignore(x => x.PlexTvShowEpisodeMediaDataPart);

    public static Faker<PlexMovieMediaDataStream> GetPlexMovieMediaDataStream(Seed seed) =>
        _plexMovieMediaDataStream.UseSeed(seed.Next());

    public static Faker<PlexTvShowEpisodeMediaDataStream> GetPlexTvShowEpisodeMediaDataStream(Seed seed) =>
        _plexTvShowEpisodeMediaDataStream.UseSeed(seed.Next());
}
