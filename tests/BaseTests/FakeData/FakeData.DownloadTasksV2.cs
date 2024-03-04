using Bogus;
using ByteSizeLib;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    #region Base

    private static Faker<T> ApplyDownloadTaskBaseV2<T>(this Faker<T> faker, int seed = 0, Action<FakeDataConfig> options = null) where T : DownloadTaskBase
    {
        var config = FakeDataConfig.FromOptions(options);
        return faker
            .StrictMode(true)
            .UseSeed(seed)
            .RuleFor(x => x.Id, _ => Guid.Empty)
            .RuleFor(x => x.Key, _ => _random.Next(1, 10000))
            .RuleFor(x => x.DataTotal, f => config.DownloadFileSizeInMb > 0
                ? (long)ByteSize.FromMebiBytes(config.DownloadFileSizeInMb).Bytes
                : f.Random.Long(1, 10000000))
            .RuleFor(x => x.DownloadStatus, _ => DownloadStatus.Queued)
            .RuleFor(x => x.Created, f => f.Date.Recent(30))
            .RuleFor(x => x.PlexServerId, _ => 0)
            .RuleFor(x => x.PlexServer, _ => null)
            .RuleFor(x => x.PlexLibraryId, _ => 0)
            .RuleFor(x => x.PlexLibrary, _ => null)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.None);
    }

    public static Faker<T> ApplyDownloadTaskParentBaseV2<T>(this Faker<T> faker, int seed = 0, Action<FakeDataConfig> options = null)
        where T : DownloadTaskParentBase
    {
        return faker
            .StrictMode(true)
            .UseSeed(seed)
            .ApplyDownloadTaskBaseV2(seed, options)
            .RuleFor(x => x.Title, f => f.Company.CompanyName())
            .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
            .RuleFor(x => x.FullTitle, f => f.Company.CompanyName())
            .RuleFor(x => x.Percentage, _ => 0)
            .RuleFor(x => x.DataReceived, _ => 0)
            .RuleFor(x => x.DownloadSpeed, _ => 0);
    }

    public static Faker<T> ApplyDownloadTaskFileBaseV2<T>(this Faker<T> faker, int seed = 0, Action<FakeDataConfig> options = null)
        where T : DownloadTaskFileBase
    {
        return faker
            .StrictMode(true)
            .UseSeed(seed)
            .ApplyDownloadTaskBaseV2(seed, options)
            .RuleFor(x => x.Percentage, _ => 0)
            .RuleFor(x => x.DataReceived, _ => 0)
            .RuleFor(x => x.DownloadSpeed, _ => 0)
            .RuleFor(x => x.FileTransferSpeed, _ => 0)
            .RuleFor(x => x.FileName, _ => "file.mp4")
            .RuleFor(x => x.FileLocationUrl, _ => PlexMockServerConfig.FileUrl)
            .RuleFor(x => x.Quality, f => f.PickRandom("sd", "720", "1080"))
            .RuleFor(x => x.DownloadDirectory, f => f.System.FilePath())
            .RuleFor(x => x.DownloadDirectory, f => f.System.FilePath())
            .RuleFor(x => x.DestinationDirectory, f => f.System.FilePath())
            .RuleFor(x => x.DownloadFolder, _ => null)
            .RuleFor(x => x.DownloadFolderId, _ => 1)
            .RuleFor(x => x.DestinationFolder, _ => null)
            .RuleFor(x => x.DestinationFolderId, _ => 2)
            .RuleFor(x => x.DownloadWorkerTasks, _ => new List<DownloadWorkerTask>());
    }

    #endregion

    #region Movie

    public static Faker<DownloadTaskMovie> GetMovieDownloadTaskV2(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTaskMovie>()
            .ApplyDownloadTaskParentBaseV2(seed, options)
            .UseSeed(seed)
            .RuleFor(x => x.MediaType, PlexMediaType.Movie)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.Movie)
            .RuleFor(x => x.DownloadStatus, _ => DownloadStatus.Queued)
            .RuleFor(x => x.Children, _ =>
            {
                if (config.IncludeMultiPartMovies)
                    return GetDownloadTaskMovieFile(seed, options).Generate(2);

                return GetDownloadTaskMovieFile(seed, options).Generate(1);
            })
            .FinishWith((_, downloadTask) =>
            {
                downloadTask.Children.ForEach(child =>
                {
                    child.Parent = downloadTask;
                    child.ParentId = child.Id;
                });
            });
    }

    public static Faker<DownloadTaskMovieFile> GetDownloadTaskMovieFile(int seed = 0, Action<FakeDataConfig> options = null)
    {
        return new Faker<DownloadTaskMovieFile>()
            .ApplyDownloadTaskFileBaseV2(seed, options)
            .UseSeed(seed)
            .RuleFor(x => x.Parent, _ => null)
            .RuleFor(x => x.ParentId, _ => Guid.Empty)
            .RuleFor(x => x.MediaType, PlexMediaType.Movie)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.MovieData);
    }

    #endregion

    #region TvShow

    public static Faker<DownloadTaskTvShow> GetDownloadTaskTvShow(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTaskTvShow>()
            .ApplyDownloadTaskParentBaseV2(seed, options)
            .UseSeed(seed)
            .RuleFor(x => x.MediaType, PlexMediaType.TvShow)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.TvShow)
            .RuleFor(x => x.Children, _ =>
            {
                var f = GetDownloadTaskTvShowSeason(seed, options);
                if (config.TvShowSeasonDownloadTasksCount > 0)
                    return f.Generate(config.TvShowSeasonDownloadTasksCount);

                return f.GenerateBetween(1, 5);
            });
    }

    public static Faker<DownloadTaskTvShowSeason> GetDownloadTaskTvShowSeason(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTaskTvShowSeason>()
            .ApplyDownloadTaskParentBaseV2(seed, options)
            .UseSeed(seed)
            .RuleFor(x => x.Parent, _ => null)
            .RuleFor(x => x.ParentId, _ => Guid.Empty)
            .RuleFor(x => x.MediaType, PlexMediaType.Season)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.Season)
            .RuleFor(x => x.Children, _ =>
            {
                var f = GetDownloadTaskTvShowEpisode(seed, options);
                if (config.TvShowEpisodeDownloadTasksCount > 0)
                    return f.Generate(config.TvShowEpisodeDownloadTasksCount);

                return f.GenerateBetween(5, 10);
            });
    }

    public static Faker<DownloadTaskTvShowEpisode> GetDownloadTaskTvShowEpisode(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTaskTvShowEpisode>()
            .ApplyDownloadTaskParentBaseV2(seed, options)
            .UseSeed(seed)
            .RuleFor(x => x.Parent, _ => null)
            .RuleFor(x => x.ParentId, _ => Guid.Empty)
            .RuleFor(x => x.MediaType, PlexMediaType.Episode)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.Episode)
            .RuleFor(x => x.Children, _ => GetDownloadTaskTvShowEpisodeFile(seed, options).Generate(1));
    }

    public static Faker<DownloadTaskTvShowEpisodeFile> GetDownloadTaskTvShowEpisodeFile(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTaskTvShowEpisodeFile>()
            .ApplyDownloadTaskFileBaseV2(seed, options)
            .UseSeed(seed)
            .RuleFor(x => x.Parent, _ => null)
            .RuleFor(x => x.ParentId, _ => Guid.Empty)
            .RuleFor(x => x.MediaType, PlexMediaType.Episode)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.EpisodeData);
    }

    #endregion

    #region DownloadWorkerTasks

    public static Faker<DownloadWorkerTask> GetDownloadWorkerTask(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        var partIndex = 1;
        return new Faker<DownloadWorkerTask>()
            .StrictMode(true)
            .UseSeed(seed)
            .RuleFor(x => x.Id, _ => 0)
            .RuleFor(x => x.FileName, f => f.System.FileName() + ".mp4")
            .RuleFor(x => x.StartByte, f => f.Random.Long(0))
            .RuleFor(x => x.EndByte, f => f.Random.Long(0))
            .RuleFor(x => x.BytesReceived, 0)
            .RuleFor(x => x.PartIndex, _ => partIndex++)
            .RuleFor(x => x.TempDirectory, f => f.System.FilePath())
            .RuleFor(x => x.ElapsedTime, 0)
            .RuleFor(x => x.FileLocationUrl, f => f.Internet.UrlRootedPath())
            .RuleFor(x => x.DownloadStatus, DownloadStatus.Queued)
            .RuleFor(x => x.DownloadTaskId, _ => 0)
            .RuleFor(x => x.DownloadTask, _ => null)
            .RuleFor(x => x.DownloadWorkerTaskLogs, new List<DownloadWorkerLog>());
    }

    #endregion
}