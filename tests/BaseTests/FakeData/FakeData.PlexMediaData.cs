using Bogus;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    private static Faker<T> ApplyBasePlexMediaData<T>(this Faker<T> faker)
        where T : BasePlexMediaData =>
        faker
            .StrictMode(true)
            .RuleFor(x => x.Id, 0)
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
            .RuleFor(x => x.PlexServerId, _ => 0)
            .RuleFor(x => x.PlexServer, _ => null)
            .RuleFor(x => x.PlexLibraryId, _ => 0)
            .RuleFor(x => x.PlexLibrary, _ => null);

    public static Faker<PlexMovieMediaData> GetPlexMovieMediaData(Seed seed, Action<FakeDataConfig>? options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return GetOrCreateCachedFaker(
                () =>
                    new Faker<PlexMovieMediaData>()
                        .StrictMode(true)
                        .ApplyBasePlexMediaData()
                        .RuleFor(
                            x => x.Parts,
                            _ => GetPlexMovieMediaDataPart(seed).Generate(config.IncludeMultiPartMovies ? 2 : 1)
                        )
                        .RuleFor(x => x.PlexMovieId, _ => 0)
                        .RuleFor(x => x.PlexMovie, _ => null)
            )
            .UseSeed(seed.Next());
    }

    public static Faker<PlexTvShowEpisodeMediaData> GetPlexTvShowEpisodeMediaData(
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        return GetOrCreateCachedFaker(
                () =>
                    new Faker<PlexTvShowEpisodeMediaData>()
                        .StrictMode(true)
                        .ApplyBasePlexMediaData()
                        .RuleFor(
                            x => x.Parts,
                            _ =>
                                GetPlexTvShowEpisodeMediaDataPart(seed)
                                    .Generate(config.IncludeMultiPartEpisodes ? 2 : 1)
                        )
                        .RuleFor(x => x.PlexTvShowEpisodeId, _ => 0)
                        .RuleFor(x => x.PlexTvShowEpisode, _ => null)
            )
            .UseSeed(seed.Next());
    }
}
