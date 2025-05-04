using Bogus;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    private static Faker<T> ApplyBasePlexMediaDataPart<T>(this Faker<T> faker)
        where T : BasePlexMediaDataPart =>
        faker
            .StrictMode(true)
            .RuleFor(x => x.Id, _ => 0)
            .RuleFor(x => x.PlexId, f => f.Random.Long(1))
            .RuleFor(x => x.Exists, true)
            .RuleFor(x => x.Accessible, f => f.Random.Bool())
            .RuleFor(x => x.Key, _ => DownloadFileUrl)
            .RuleFor(x => x.Duration, f => f.Random.Int(50000, 5512400))
            .RuleFor(x => x.AudioProfile, _ => "dts")
            .RuleFor(x => x.File, _ => "/file.mp4")
            .RuleFor(x => x.Size, _ => 50 * 1024)
            .RuleFor(x => x.Container, f => f.System.FileExt("video/mp4"))
            .RuleFor(x => x.VideoProfile, f => f.Random.Words(2))
            .RuleFor(x => x.Indexes, f => f.Random.Word())
            .RuleFor(x => x.PlexServerId, _ => 0)
            .Ignore(x => x.PlexServer)
            .RuleFor(x => x.PlexLibraryId, _ => 0)
            .Ignore(x => x.PlexLibrary);

    private static readonly Faker<PlexMovieMediaDataPart> _plexMovieMediaDataPart = new Faker<PlexMovieMediaDataPart>()
        .ApplyBasePlexMediaDataPart()
        .RuleFor(x => x.PlexMovieId, _ => 0)
        .Ignore(x => x.PlexMovie)
        .RuleFor(x => x.PlexMovieMediaDataId, _ => 0)
        .Ignore(x => x.PlexMovieMediaData);

    private static readonly Faker<PlexTvShowEpisodeMediaDataPart> _plexTvShowEpisodeMediaDataPart =
        new Faker<PlexTvShowEpisodeMediaDataPart>()
            .ApplyBasePlexMediaDataPart()
            .RuleFor(x => x.PlexTvShowEpisodeId, _ => 0)
            .Ignore(x => x.PlexTvShowEpisode)
            .RuleFor(x => x.PlexTvShowEpisodeMediaDataId, _ => 0)
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
