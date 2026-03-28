using ByteSizeLib;
using Reaparr.Application;

namespace Reaparr.BaseTests;

public static partial class FakeData
{
    private static Faker<DownloadTaskTvShowEpisode> CreateDownloadTaskTvShowEpisodeFaker()
    {
        return new Faker<DownloadTaskTvShowEpisode>()
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
                    {
                        var currentFileIndex = fileIndex++;
                        file.FullTitle = $"{episode.FullTitle}/{currentFileIndex}-{file.FileName}";
                    }
                }
            );
    }

    public static Faker<DownloadTaskTvShowEpisode> GetDownloadTaskTvShowEpisode(
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        return CreateDownloadTaskTvShowEpisodeFaker()
            .UseSeed(seed.Next())
            .RuleFor(
                x => x.DataTotal,
                (_, x) =>
                    config.DownloadFileSizeInMb > 0
                        ? (long)ByteSize.FromMebiBytes(config.DownloadFileSizeInMb).Bytes
                        : x.DataTotal
            )
            .RuleFor(
                x => x.Children,
                _ => GetDownloadTaskTvShowEpisodeFile(seed, options).Generate(config.IncludeMultiPartEpisodes ? 2 : 1)
            );
    }

    private static Faker<DownloadTaskTvShowEpisodeFile> CreateDownloadTaskTvShowEpisodeFileFaker()
    {
        return new Faker<DownloadTaskTvShowEpisodeFile>()
            .ApplyDownloadTaskFileBase(DownloadTaskType.EpisodeData)
            .Ignore(x => x.Parent)
            .Ignore(x => x.ParentId)
            .Ignore(x => x.Logs);
    }

    public static Faker<DownloadTaskTvShowEpisodeFile> GetDownloadTaskTvShowEpisodeFile(
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        return CreateDownloadTaskTvShowEpisodeFileFaker()
            .RuleFor(
                x => x.DataTotal,
                f =>
                    config.DownloadFileSizeInMb > 0
                        ? (long)ByteSize.FromMebiBytes(config.DownloadFileSizeInMb).Bytes
                        : f.Random.Long(1, 10000000)
            )
            .UseSeed(seed.Next());
    }
}
