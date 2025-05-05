using Bogus;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    private static Faker<T> ApplyBasePlexMediaData<T>(this Faker<T> faker)
        where T : BasePlexMediaData =>
        faker
            .StrictMode(true)
            .Ignore(x => x.Id)
            .RuleFor(x => x.PlexId, f => f.Random.Long(1))
            .RuleFor(x => x.Bitrate, f => f.Random.Int(1900, 2030))
            .RuleFor(x => x.Width, f => f.Random.Int(240, 10000))
            .RuleFor(x => x.Height, f => f.Random.Int(240, 10000))
            .RuleFor(x => x.VideoFrameRate, _ => "24p")
            .RuleFor(x => x.VideoProfile, _ => "high")
            .RuleFor(x => x.AudioCodec, _ => "dca")
            .RuleFor(x => x.AudioProfile, _ => "dts")
            .RuleFor(x => x.AspectRatio, f => f.Random.Float(1, 2))
            .RuleFor(x => x.VideoCodec, f => f.System.FileType())
            .RuleFor(x => x.AudioChannels, f => f.Random.Int(2, 5))
            .RuleFor(x => x.VideoResolution, f => f.PickRandom("sd", "720p", "1080p"))
            .RuleFor(x => x.Duration, f => f.Random.Int(50000, 55124400))
            .RuleFor(x => x.Container, f => f.System.FileType())
            .RuleFor(x => x.HasVoiceActivity, f => f.Random.Bool())
            .Ignore(x => x.PlexServerId)
            .Ignore(x => x.PlexServer)
            .Ignore(x => x.PlexLibraryId)
            .Ignore(x => x.PlexLibrary);

    private static readonly Faker<PlexMovieMediaData> _plexMovieMediaData = new Faker<PlexMovieMediaData>()
        .ApplyBasePlexMediaData()
        .Ignore(x => x.PlexMovieId)
        .Ignore(x => x.PlexMovie);

    private static readonly Faker<PlexTvShowEpisodeMediaData> _plexTvShowEpisodeMediaData =
        new Faker<PlexTvShowEpisodeMediaData>()
            .ApplyBasePlexMediaData()
            .Ignore(x => x.PlexTvShowEpisodeId)
            .Ignore(x => x.PlexTvShowEpisode);

    public static Faker<PlexMovieMediaData> GetPlexMovieMediaData(Seed seed, Action<FakeDataConfig>? options = null)
    {
        var config = FakeDataConfig.FromOptions(options);
        return _plexMovieMediaData
            .RuleFor(x => x.Parts, _ => GetPlexMovieMediaDataPart(seed).Generate(config.IncludeMultiPartMovies ? 2 : 1))
            .UseSeed(seed.Next());
    }

    public static Faker<PlexTvShowEpisodeMediaData> GetPlexTvShowEpisodeMediaData(
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);
        return _plexTvShowEpisodeMediaData
            .RuleFor(
                x => x.Parts,
                _ => GetPlexTvShowEpisodeMediaDataPart(seed).Generate(config.IncludeMultiPartEpisodes ? 2 : 1)
            )
            .UseSeed(seed.Next());
    }
}
