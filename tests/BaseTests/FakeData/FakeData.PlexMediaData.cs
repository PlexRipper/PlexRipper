namespace Reaparr.BaseTests;

public static partial class FakeData
{
    private static Faker<T> ApplyBasePlexMediaData<T>(this Faker<T> faker)
        where T : BasePlexMediaData =>
        faker
            .StrictMode(true)
            .Ignore(x => x.Id)
            .RuleFor(x => x.PlexMediaId, f => f.Random.Long(1))
            .RuleFor(x => x.PlexPartId, f => f.Random.Long(1))
            .RuleFor(x => x.VideoCodec, f => f.System.FileType())
            .RuleFor(x => x.AudioCodec, _ => "dca")
            .RuleFor(x => x.VideoResolution, f => f.PickRandom("sd", "720p", "1080p"))
            .RuleFor(x => x.Quality, (_, x) => x.VideoResolution.ToVideoQuality())
            .RuleFor(x => x.Duration, f => f.Random.Int(50000, 55124400))
            .RuleFor(x => x.Container, f => f.System.FileType())
            .RuleFor(x => x.Source, _ => ReleaseSource.WebDl)
            .RuleFor(x => x.NeedsGeneratedName, _ => false)
            .RuleFor(x => x.GeneratedNameSyncedAt, _ => DateTime.UtcNow)
            .RuleFor(x => x.OriginalFilename, f => f.System.FileName("video"))
            .RuleFor(x => x.GeneratedFilename, _ => string.Empty)
            .RuleFor(x => x.RatingKey, f => f.Random.Int(1, 100000))
            .RuleFor(x => x.Key, _ => DownloadFileUrl)
            .RuleFor(x => x.Size, _ => 50 * 1024)
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
        return _plexMovieMediaData.UseSeed(seed.Next());
    }

    public static Faker<PlexTvShowEpisodeMediaData> GetPlexTvShowEpisodeMediaData(
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);
        return _plexTvShowEpisodeMediaData.UseSeed(seed.Next());
    }
}
