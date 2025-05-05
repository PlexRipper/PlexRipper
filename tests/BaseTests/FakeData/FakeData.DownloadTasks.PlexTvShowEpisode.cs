using ByteSizeLib;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    private static readonly Faker<DownloadTaskTvShowEpisode> _downloadTaskTvShowEpisode =
        new Faker<DownloadTaskTvShowEpisode>()
            .ApplyDownloadTaskParentBase(DownloadTaskType.Episode)
            .Ignore(x => x.Parent)
            .Ignore(x => x.ParentId)
            .RuleFor(x => x.Title, _ => "Episode")
            .RuleFor(x => x.FullTitle, _ => "Episode")
            .Ignore(x => x.Children)
            .FinishWith(
                (_, episode) =>
                {
                    var fileIndex = 1;
                    foreach (var file in episode.Children)
                        file.FullTitle = $"{episode.FullTitle}/{fileIndex}-{file.FileName}";
                }
            );

    public static Faker<DownloadTaskTvShowEpisode> GetDownloadTaskTvShowEpisode(
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        return _downloadTaskTvShowEpisode
            .UseSeed(seed.Next())
            .RuleFor(
                x => x.DataTotal,
                (_, x) =>
                    config.DownloadFileSizeInMb > 0
                        ? (long)ByteSize.FromMebiBytes(config.DownloadFileSizeInMb).Bytes
                        : x.DataTotal
            )
            .RuleFor(x => x.Children, _ => [GetDownloadTaskTvShowEpisodeFile(seed, options).Generate()]);
    }

    private static readonly Faker<DownloadTaskTvShowEpisodeFile> _downloadTaskTvShowEpisodeFile =
        new Faker<DownloadTaskTvShowEpisodeFile>()
            .ApplyDownloadTaskFileBase(DownloadTaskType.EpisodeData)
            .Ignore(x => x.Parent)
            .Ignore(x => x.ParentId);

    public static Faker<DownloadTaskTvShowEpisodeFile> GetDownloadTaskTvShowEpisodeFile(
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        return _downloadTaskTvShowEpisodeFile
            .RuleFor(
                x => x.DataTotal,
                f =>
                    config.DownloadFileSizeInMb > 0
                        ? (long)ByteSize.FromMebiBytes(config.DownloadFileSizeInMb).Bytes
                        : f.Random.Long(1, 10000000)
            )
            .UseSeed(seed.Next())
            .FinishWith(
                (_, x) =>
                {
                    // This has to run last and can therefore not be run in a RuleFor
                    x.DownloadWorkerTasks = x.GenerateDownloadWorkerTasks(config.DownloadWorkerTasks);
                }
            );
    }
}
