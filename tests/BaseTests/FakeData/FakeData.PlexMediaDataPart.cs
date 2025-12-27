namespace Reaparr.BaseTests;

public static partial class FakeData
{
    private static Faker<T> ApplyBasePlexMediaDataPart<T>(this Faker<T> faker)
        where T : BasePlexMediaDataPart =>
        faker
            .StrictMode(true)
            .Ignore(x => x.Id)
            .RuleFor(x => x.PlexId, f => f.Random.Long(1))
            .RuleFor(x => x.Key, _ => DownloadFileUrl)
            .RuleFor(x => x.Duration, f => f.Random.Int(50000, 5512400))
            .RuleFor(x => x.AudioProfile, _ => "dts")
            .RuleFor(x => x.File, _ => "/file.mp4")
            .RuleFor(x => x.Size, _ => 50 * 1024)
            .RuleFor(x => x.Container, f => f.System.FileExt("video/mp4"))
            .RuleFor(x => x.VideoProfile, f => f.Random.Words(2))
            .RuleFor(x => x.Indexes, f => f.Random.Word())
            .Ignore(x => x.PlexServerId)
            .Ignore(x => x.PlexServer)
            .Ignore(x => x.PlexLibraryId)
            .Ignore(x => x.PlexLibrary);

    private static readonly Faker<PlexMovieMediaDataPart> _plexMovieMediaDataPart = new Faker<PlexMovieMediaDataPart>()
        .ApplyBasePlexMediaDataPart()
        .Ignore(x => x.PlexMovieId)
        .Ignore(x => x.PlexMovie)
        .Ignore(x => x.PlexMovieMediaDataId)
        .Ignore(x => x.PlexMovieMediaData);

    private static readonly Faker<PlexTvShowEpisodeMediaDataPart> _plexTvShowEpisodeMediaDataPart =
        new Faker<PlexTvShowEpisodeMediaDataPart>()
            .ApplyBasePlexMediaDataPart()
            .Ignore(x => x.PlexTvShowEpisodeId)
            .Ignore(x => x.PlexTvShowEpisode)
            .Ignore(x => x.PlexTvShowEpisodeMediaDataId)
            .Ignore(x => x.PlexTvShowEpisodeMediaData);

    public static Faker<PlexMovieMediaDataPart> GetPlexMovieMediaDataPart(Seed seed) =>
        _plexMovieMediaDataPart
            .RuleFor(x => x.Streams, _ => GetPlexMovieMediaDataStream(seed).Generate(2))
            .UseSeed(seed.Next());

    public static Faker<PlexTvShowEpisodeMediaDataPart> GetPlexTvShowEpisodeMediaDataPart(Seed seed) =>
        _plexTvShowEpisodeMediaDataPart
            .RuleFor(x => x.Streams, _ => GetPlexTvShowEpisodeMediaDataStream(seed).Generate(2))
            .UseSeed(seed.Next());
}
