namespace Reaparr.BaseTests;

public static partial class FakeData
{
    private static Faker<T> ApplyBasePlexMediaData<T>(this Faker<T> faker, FakeDataConfig config)
        where T : BasePlexMediaData =>
        faker
            .StrictMode(true)
            .Ignore(x => x.Id)
            .RuleFor(x => x.PlexApiMediaId, _ => GetUniqueNumber())
            .RuleFor(x => x.PlexApiPartId, _ => GetUniqueNumber())
            .RuleFor(x => x.VideoCodec, f => f.System.FileType())
            .RuleFor(x => x.AudioCodec, _ => "dca")
            .RuleFor(x => x.VideoResolution, f => f.PickRandom(VideoQuality.SD, VideoQuality.HD, VideoQuality.FullHD))
            .RuleFor(x => x.Quality, (_, x) => x.VideoResolution)
            .RuleFor(x => x.Duration, f => f.Random.Int(50000, 55124400))
            .RuleFor(x => x.Container, f => f.System.FileType())
            .RuleFor(x => x.Source, _ => ReleaseSource.WebDl)
            .RuleFor(x => x.NeedsGeneratedName, _ => false)
            .RuleFor(x => x.GeneratedNameSyncedAt, _ => DateTime.UtcNow)
            .RuleFor(x => x.OriginalFilename, f => f.System.FileName("video"))
            .RuleFor(x => x.GeneratedFilename, _ => string.Empty)
            .RuleFor(x => x.PlexApiRatingKey, _ => GetUniqueNumber())
            .RuleFor(x => x.Key, _ => DownloadFileUrl)
            .RuleFor(
                x => x.Size,
                _ =>
                    config.DownloadFileSizeInMb > 0
                        ? (long)ByteSize.FromMebiBytes(config.DownloadFileSizeInMb).Bytes
                        : 50 * 1024
            )
            .Ignore(x => x.PlexServerId)
            .Ignore(x => x.PlexServer)
            .Ignore(x => x.PlexLibraryId)
            .Ignore(x => x.PlexLibrary);

    private static Faker<PlexMovieMediaData> CreatePlexMovieMediaDataFaker(FakeDataConfig config)
    {
        return new Faker<PlexMovieMediaData>()
            .ApplyBasePlexMediaData(config)
            .Ignore(x => x.PlexMovieId)
            .Ignore(x => x.PlexMovie);
    }

    private static Faker<PlexTvShowEpisodeMediaData> CreatePlexTvShowEpisodeMediaDataFaker(FakeDataConfig config)
    {
        return new Faker<PlexTvShowEpisodeMediaData>()
            .ApplyBasePlexMediaData(config)
            .Ignore(x => x.PlexTvShowEpisodeId)
            .Ignore(x => x.PlexTvShowEpisode);
    }

    public static Faker<PlexMovieMediaData> GetPlexMovieMediaData(Seed seed, Action<FakeDataConfig>? options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return CreatePlexMovieMediaDataFaker(config).UseSeed(seed.Next());
    }

    public static Faker<PlexTvShowEpisodeMediaData> GetPlexTvShowEpisodeMediaData(
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        return CreatePlexTvShowEpisodeMediaDataFaker(config).UseSeed(seed.Next());
    }
}
