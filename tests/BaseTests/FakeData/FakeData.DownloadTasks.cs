using Bogus;
using ByteSizeLib;
using Environment;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    #region Base

    private static Faker<T> ApplyDownloadTaskBase<T>(this Faker<T> faker, Seed seed, DownloadTaskType downloadTaskType)
        where T : DownloadTaskBase
    {
        return faker
            .StrictMode(true)
            .UseSeed(seed.Next())
            .Ignore(x => x.Id)
            .RuleFor(x => x.Key, _ => GetUniqueNumber())
            .RuleFor(x => x.DownloadTaskType, downloadTaskType)
            .RuleFor(x => x.MediaType, (_, x) => x.DownloadTaskType.ToPlexMediaType())
            .RuleFor(
                x => x.Title,
                f =>
                {
                    if (downloadTaskType == DownloadTaskType.Movie)
                        return "Movie " + f.Random.Int(1, 10000);

                    if (downloadTaskType == DownloadTaskType.TvShow)
                        return "TvShow " + f.Random.Int(1, 10000);

                    return f.Company.CompanyName();
                }
            )
            .RuleFor(x => x.FullTitle, (_, x) => x.Title)
            .RuleFor(x => x.DownloadStatus, _ => DownloadStatus.Queued)
            .RuleFor(x => x.CreatedAt, _ => DateTime.UtcNow)
            .Ignore(x => x.PlexServerId)
            .Ignore(x => x.PlexServer)
            .Ignore(x => x.PlexLibraryId)
            .Ignore(x => x.PlexLibrary);
    }

    private static Faker<T> ApplyDownloadTaskParentBase<T>(
        this Faker<T> faker,
        Seed seed,
        DownloadTaskType downloadTaskType,
        Action<FakeDataConfig>? options = null
    )
        where T : DownloadTaskParentBase
    {
        var config = FakeDataConfig.FromOptions(options);

        return faker
            .StrictMode(true)
            .UseSeed(seed.Next())
            .ApplyDownloadTaskBase(seed, downloadTaskType)
            .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
            .Ignore(x => x.FileTransferSpeed)
            .Ignore(x => x.DataReceived)
            .Ignore(x => x.FileDataTransferred)
            .Ignore(x => x.DownloadSpeed)
            .RuleFor(
                x => x.DataTotal,
                f =>
                    config.DownloadFileSizeInMb > 0
                        ? (long)ByteSize.FromMebiBytes(config.DownloadFileSizeInMb).Bytes
                        : f.Random.Long(1, 10000000)
            );
    }

    private static Faker<T> ApplyDownloadTaskFileBase<T>(
        this Faker<T> faker,
        Seed seed,
        DownloadTaskType downloadTaskType,
        Action<FakeDataConfig>? options = null
    )
        where T : DownloadTaskFileBase
    {
        var config = FakeDataConfig.FromOptions(options);

        return faker
            .StrictMode(true)
            .UseSeed(seed.Next())
            .ApplyDownloadTaskBase(seed, downloadTaskType)
            .Ignore(x => x.DataReceived)
            .RuleFor(
                x => x.DataTotal,
                f =>
                    config.DownloadFileSizeInMb > 0
                        ? (long)ByteSize.FromMebiBytes(config.DownloadFileSizeInMb).Bytes
                        : f.Random.Long(1, 10000000)
            )
            .Ignore(x => x.DownloadSpeed)
            .Ignore(x => x.FileTransferSpeed)
            .Ignore(x => x.FileDataTransferred)
            .Ignore(x => x.CurrentFileTransferPathIndex)
            .Ignore(x => x.CurrentFileTransferBytesOffset)
            .Ignore(x => x.DestinationFolderPathId)
            .RuleFor(x => x.Quality, f => f.PickRandom("sd", "720p", "1080p", "2160p"))
            .RuleFor(
                x => x.FileName,
                (_, x) =>
                {
                    if (x.DownloadTaskType == DownloadTaskType.MovieData)
                        return $"movie-{x.Title.SanitizeFolderName()}.[{x.Quality}].file.mp4";

                    if (x.DownloadTaskType == DownloadTaskType.EpisodeData)
                        return $"episode-{x.Title.SanitizeFolderName()}.[{x.Quality}].file.mp4";

                    return $"{x.Title.SanitizeFolderName()}.[{x.Quality}].file.mp4";
                }
            )
            .RuleFor(x => x.FileLocationUrl, _ => DownloadFileUrl)
            .RuleFor(
                x => x.DirectoryMeta,
                (_, x) =>
                    new DownloadTaskDirectory
                    {
                        DestinationRootPath = x.MediaType.ToDefaultDestinationLocation(),
                        DownloadRootPath = PathProvider.DefaultDownloadsDestinationFolder,
                        MovieFolder = x.Title,
                        TvShowFolder = string.Empty,
                        SeasonFolder = string.Empty,
                    }
            )
            .RuleFor(
                x => x.DownloadWorkerTasks,
                (_, task) => task.GenerateDownloadWorkerTasks(config.DownloadWorkerTasks)
            );
    }

    #endregion

    #region Movie

    public static Faker<DownloadTaskMovie> GetMovieDownloadTask(Seed seed, Action<FakeDataConfig>? options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTaskMovie>()
            .UseSeed(seed.Next())
            .ApplyDownloadTaskParentBase(seed, DownloadTaskType.Movie, options)
            .RuleFor(x => x.MediaType, PlexMediaType.Movie)
            .RuleFor(
                x => x.Children,
                _ =>
                {
                    if (config.IncludeMultiPartMovies)
                        return GetDownloadTaskMovieFile(seed, options).Generate(2);

                    return GetDownloadTaskMovieFile(seed, options).Generate(1);
                }
            )
            .FinishWith(
                (_, movie) =>
                {
                    var movieIndex = 1;
                    foreach (var movieFile in movie.Children)
                    {
                        movieFile.Title = $"{movieFile.Title} {movieIndex++}";
                        movieFile.FullTitle = $"{movie.FullTitle}/{movieIndex}-{movieFile.FileName}";
                    }
                }
            );
    }

    public static Faker<DownloadTaskMovieFile> GetDownloadTaskMovieFile(
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        return new Faker<DownloadTaskMovieFile>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .ApplyDownloadTaskFileBase(seed, DownloadTaskType.MovieData, options)
            .Ignore(x => x.Parent)
            .Ignore(x => x.ParentId);
    }

    #endregion

    #region TvShow

    public static Faker<DownloadTaskTvShow> GetDownloadTaskTvShow(Seed seed, Action<FakeDataConfig>? options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTaskTvShow>()
            .UseSeed(seed.Next())
            .StrictMode(true)
            .ApplyDownloadTaskParentBase(seed, DownloadTaskType.TvShow, options)
            .RuleFor(
                x => x.Children,
                _ =>
                {
                    var f = GetDownloadTaskTvShowSeason(seed, options);
                    if (config.TvShowSeasonDownloadTasksCount > 0)
                        return f.Generate(config.TvShowSeasonDownloadTasksCount);

                    return f.GenerateBetween(1, 5);
                }
            )
            .FinishWith(
                (_, tvShow) =>
                {
                    var seasonIndex = 1;

                    foreach (var season in tvShow.Children)
                    {
                        season.Title = $"{season.Title} {seasonIndex++}";
                        season.FullTitle = $"{tvShow.FullTitle}/{season.Title}";

                        foreach (var episode in season.Children)
                        {
                            episode.FullTitle = $"{season.FullTitle}/{episode.Title}";

                            var fileIndex = 1;
                            foreach (var file in episode.Children)
                            {
                                file.FullTitle = $"{episode.FullTitle}/{fileIndex}-{file.FileName}";
                                file.DirectoryMeta.TvShowFolder = tvShow.Title;
                                file.DirectoryMeta.SeasonFolder = season.Title;
                            }
                        }
                    }
                }
            );
    }

    public static Faker<DownloadTaskTvShowSeason> GetDownloadTaskTvShowSeason(
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTaskTvShowSeason>()
            .UseSeed(seed.Next())
            .StrictMode(true)
            .ApplyDownloadTaskParentBase(seed, DownloadTaskType.Season, options)
            .Ignore(x => x.ParentId)
            .RuleFor(x => x.Title, _ => "Season")
            .RuleFor(x => x.FullTitle, _ => "Season")
            .Ignore(x => x.Parent)
            .RuleFor(
                x => x.Children,
                _ =>
                {
                    var f = GetDownloadTaskTvShowEpisode(seed, options);
                    if (config.TvShowEpisodeDownloadTasksCount > 0)
                        return f.Generate(config.TvShowEpisodeDownloadTasksCount);

                    return f.GenerateBetween(5, 10);
                }
            )
            .FinishWith(
                (_, season) =>
                {
                    season.FullTitle = season.Title;

                    var episodeIndex = 1;
                    foreach (var episode in season.Children)
                    {
                        episode.Title = $"{episode.Title} {episodeIndex++}";
                        episode.FullTitle = $"{season.FullTitle}/{episode.Title}";

                        var fileIndex = 1;
                        foreach (var file in episode.Children)
                        {
                            file.FullTitle = $"{episode.FullTitle}/{fileIndex}-{file.FileName}";
                            file.DirectoryMeta.SeasonFolder = season.Title;
                        }
                    }
                }
            );
    }

    public static Faker<DownloadTaskTvShowEpisode> GetDownloadTaskTvShowEpisode(
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        return new Faker<DownloadTaskTvShowEpisode>()
            .UseSeed(seed.Next())
            .StrictMode(true)
            .ApplyDownloadTaskParentBase(seed, DownloadTaskType.Episode, options)
            .Ignore(x => x.Parent)
            .Ignore(x => x.ParentId)
            .RuleFor(x => x.Title, _ => "Episode")
            .RuleFor(x => x.FullTitle, _ => "Episode")
            .RuleFor(x => x.Children, _ => GetDownloadTaskTvShowEpisodeFile(seed, options).Generate(1))
            .FinishWith(
                (_, episode) =>
                {
                    var fileIndex = 1;
                    foreach (var file in episode.Children)
                        file.FullTitle = $"{episode.FullTitle}/{fileIndex}-{file.FileName}";
                }
            );
    }

    public static Faker<DownloadTaskTvShowEpisodeFile> GetDownloadTaskTvShowEpisodeFile(
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        return new Faker<DownloadTaskTvShowEpisodeFile>()
            .UseSeed(seed.Next())
            .StrictMode(true)
            .ApplyDownloadTaskFileBase(seed, DownloadTaskType.EpisodeData, options)
            .Ignore(x => x.Parent)
            .Ignore(x => x.ParentId);
    }

    #endregion
}
