using Bogus;
using ByteSizeLib;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    #region Base

    private static Faker<T> ApplyDownloadTaskBase<T>(this Faker<T> faker, int seed = 0, Action<FakeDataConfig> options = null) where T : DownloadTaskBase
    {
        return faker
            .StrictMode(true)
            .UseSeed(seed)
            .RuleFor(x => x.Id, _ => Guid.Empty)
            .RuleFor(x => x.Key, _ => _random.Next(1, 10000))
            .RuleFor(x => x.Title, _ => "")
            .RuleFor(x => x.FullTitle, _ => "")
            .RuleFor(x => x.DownloadStatus, _ => DownloadStatus.Queued)
            .RuleFor(x => x.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(x => x.PlexServerId, _ => 0)
            .RuleFor(x => x.PlexServer, _ => null)
            .RuleFor(x => x.PlexLibraryId, _ => 0)
            .RuleFor(x => x.PlexLibrary, _ => null)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.None);
    }

    public static Faker<T> ApplyDownloadTaskParentBase<T>(this Faker<T> faker, int seed = 0, Action<FakeDataConfig> options = null)
        where T : DownloadTaskParentBase
    {
        var config = FakeDataConfig.FromOptions(options);

        return faker
            .StrictMode(true)
            .UseSeed(seed)
            .ApplyDownloadTaskBase(seed, options)
            .RuleFor(x => x.Title, f => f.Company.CompanyName())
            .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
            .RuleFor(x => x.FileTransferSpeed, _ => 0)
            .RuleFor(x => x.Percentage, _ => 0)
            .RuleFor(x => x.DataReceived, _ => 0)
            .RuleFor(x => x.DownloadSpeed, _ => 0)
            .RuleFor(x => x.DataTotal, f => config.DownloadFileSizeInMb > 0
                ? (long)ByteSize.FromMebiBytes(config.DownloadFileSizeInMb).Bytes
                : f.Random.Long(1, 10000000));
    }

    public static Faker<T> ApplyDownloadTaskFileBase<T>(this Faker<T> faker, int seed = 0, Action<FakeDataConfig> options = null)
        where T : DownloadTaskFileBase
    {
        var config = FakeDataConfig.FromOptions(options);

        return faker
            .StrictMode(true)
            .UseSeed(seed)
            .ApplyDownloadTaskBase(seed, options)
            .RuleFor(x => x.Percentage, _ => 0)
            .RuleFor(x => x.DataReceived, _ => 0)
            .RuleFor(x => x.DataTotal, f => config.DownloadFileSizeInMb > 0
                ? (long)ByteSize.FromMebiBytes(config.DownloadFileSizeInMb).Bytes
                : f.Random.Long(1, 10000000))
            .RuleFor(x => x.DownloadSpeed, _ => 0)
            .RuleFor(x => x.FileTransferSpeed, _ => 0)
            .RuleFor(x => x.FileName, _ => "file.mp4")
            .RuleFor(x => x.FileLocationUrl, _ => PlexMockServerConfig.FileUrl)
            .RuleFor(x => x.Quality, f => f.PickRandom("sd", "720", "1080"))
            .RuleFor(x => x.DirectoryMeta, _ => new DownloadTaskDirectory
            {
                DestinationRootPath = string.Empty,
                DownloadRootPath = string.Empty,
                MovieFolder = string.Empty,
                TvShowFolder = string.Empty,
                SeasonFolder = string.Empty,
            })
            .RuleFor(x => x.DownloadWorkerTasks, _ => new List<DownloadWorkerTask>());
    }

    #endregion

    #region Movie

    public static Faker<DownloadTaskMovie> GetMovieDownloadTask(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTaskMovie>()
            .ApplyDownloadTaskParentBase(seed, options)
            .UseSeed(seed)
            .RuleFor(x => x.MediaType, PlexMediaType.Movie)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.Movie)
            .RuleFor(x => x.DownloadStatus, _ => DownloadStatus.Queued)
            .RuleFor(x => x.Title, f => "Movie " + f.Random.Int(1, 10000))
            .RuleFor(x => x.Children, _ =>
            {
                if (config.IncludeMultiPartMovies)
                    return GetDownloadTaskMovieFile(seed, options).Generate(2);

                return GetDownloadTaskMovieFile(seed, options).Generate(1);
            })
            .FinishWith((_, movie) =>
            {
                var movieIndex = 1;
                movie.FullTitle = movie.Title;
                movie.Children.ForEach(movieFile =>
                {
                    movieFile.Title = $"{movieFile.Title} {movieIndex++}";
                    movieFile.FullTitle = $"{movie.FullTitle}/{movieIndex}-{movieFile.FileName}";
                    movieFile.DirectoryMeta.MovieFolder = movie.Title;
                });
            });
    }

    public static Faker<DownloadTaskMovieFile> GetDownloadTaskMovieFile(int seed = 0, Action<FakeDataConfig> options = null)
    {
        return new Faker<DownloadTaskMovieFile>()
            .ApplyDownloadTaskFileBase(seed, options)
            .UseSeed(seed)
            .RuleFor(x => x.Parent, _ => null)
            .RuleFor(x => x.ParentId, _ => Guid.Empty)
            .RuleFor(x => x.MediaType, PlexMediaType.Movie)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.MovieData)
            .FinishWith((f, movieFile) =>
            {
                movieFile.FileName = $"[{movieFile.Quality}].{f.System.FileName("mp4")}";
                movieFile.Title = movieFile.FileName;
                movieFile.FullTitle = movieFile.FileName;
            });
    }

    #endregion

    #region TvShow

    public static Faker<DownloadTaskTvShow> GetDownloadTaskTvShow(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTaskTvShow>()
            .UseSeed(seed)
            .StrictMode(true)
            .ApplyDownloadTaskParentBase(seed, options)
            .RuleFor(x => x.MediaType, PlexMediaType.TvShow)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.TvShow)
            .RuleFor(x => x.Title, f => "TvShow " + f.Random.Int(1, 10000))
            .RuleFor(x => x.Children, _ =>
            {
                var f = GetDownloadTaskTvShowSeason(seed, options);
                if (config.TvShowSeasonDownloadTasksCount > 0)
                    return f.Generate(config.TvShowSeasonDownloadTasksCount);

                return f.GenerateBetween(1, 5);
            })
            .FinishWith((_, tvShow) =>
            {
                var seasonIndex = 1;
                tvShow.FullTitle = tvShow.Title;
                tvShow.Children.ForEach(season =>
                {
                    season.Title = $"{season.Title} {seasonIndex++}";
                    season.FullTitle = $"{tvShow.FullTitle}/{season.Title}";

                    season.Children.ForEach(episode =>
                    {
                        episode.FullTitle = $"{season.FullTitle}/{episode.Title}";

                        var fileIndex = 1;
                        episode.Children.ForEach(file =>
                        {
                            file.FullTitle = $"{episode.FullTitle}/{fileIndex}-{file.FileName}";
                            file.DirectoryMeta.TvShowFolder = tvShow.Title;
                            file.DirectoryMeta.SeasonFolder = season.Title;
                        });
                    });
                });
            });

        ;
    }

    public static Faker<DownloadTaskTvShowSeason> GetDownloadTaskTvShowSeason(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTaskTvShowSeason>()
            .UseSeed(seed)
            .StrictMode(true)
            .ApplyDownloadTaskParentBase(seed, options)
            .RuleFor(x => x.Parent, _ => null)
            .RuleFor(x => x.ParentId, _ => Guid.Empty)
            .RuleFor(x => x.MediaType, PlexMediaType.Season)
            .RuleFor(x => x.Title, _ => "Season")
            .RuleFor(x => x.FullTitle, _ => "Season")
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.Season)
            .RuleFor(x => x.Children, _ =>
            {
                var f = GetDownloadTaskTvShowEpisode(seed, options);
                if (config.TvShowEpisodeDownloadTasksCount > 0)
                    return f.Generate(config.TvShowEpisodeDownloadTasksCount);

                return f.GenerateBetween(5, 10);
            })
            .FinishWith((_, season) =>
            {
                season.FullTitle = season.Title;

                var episodeIndex = 1;
                season.Children.ForEach(episode =>
                {
                    episode.Title = $"{episode.Title} {episodeIndex++}";
                    episode.FullTitle = $"{season.FullTitle}/{episode.Title}";

                    var fileIndex = 1;
                    episode.Children.ForEach(file =>
                    {
                        file.FullTitle = $"{episode.FullTitle}/{fileIndex}-{file.FileName}";
                        file.DirectoryMeta.SeasonFolder = season.Title;
                    });
                });
            });
    }

    public static Faker<DownloadTaskTvShowEpisode> GetDownloadTaskTvShowEpisode(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTaskTvShowEpisode>()
            .UseSeed(seed)
            .StrictMode(true)
            .ApplyDownloadTaskParentBase(seed, options)
            .RuleFor(x => x.Parent, _ => null)
            .RuleFor(x => x.ParentId, _ => Guid.Empty)
            .RuleFor(x => x.Title, _ => "Episode")
            .RuleFor(x => x.FullTitle, _ => "Episode")
            .RuleFor(x => x.MediaType, PlexMediaType.Episode)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.Episode)
            .RuleFor(x => x.Children, _ => GetDownloadTaskTvShowEpisodeFile(seed, options).Generate(1))
            .FinishWith((_, episode) =>
            {
                var fileIndex = 1;
                episode.Children.ForEach(file => { file.FullTitle = $"{episode.FullTitle}/{fileIndex}-{file.FileName}"; });
            });
    }

    public static Faker<DownloadTaskTvShowEpisodeFile> GetDownloadTaskTvShowEpisodeFile(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTaskTvShowEpisodeFile>()
            .UseSeed(seed)
            .StrictMode(true)
            .ApplyDownloadTaskFileBase(seed, options)
            .RuleFor(x => x.Parent, _ => null)
            .RuleFor(x => x.ParentId, _ => Guid.Empty)
            .RuleFor(x => x.MediaType, PlexMediaType.Episode)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.EpisodeData)
            .FinishWith((f, episodeFile) =>
            {
                episodeFile.FileName = $"[{episodeFile.Quality}].{f.System.FileName("mp4")}";
                episodeFile.Title = episodeFile.FileName;
                episodeFile.FullTitle = episodeFile.FileName;
            });
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
            .RuleFor(x => x.DownloadDirectory, f => f.System.FilePath())
            .RuleFor(x => x.ElapsedTime, 0)
            .RuleFor(x => x.FileLocationUrl, f => f.Internet.UrlRootedPath())
            .RuleFor(x => x.DownloadStatus, DownloadStatus.Queued)
            .RuleFor(x => x.DownloadTaskId, _ => Guid.Empty)
            .RuleFor(x => x.DownloadWorkerTaskLogs, new List<DownloadWorkerLog>());
    }

    #endregion
}