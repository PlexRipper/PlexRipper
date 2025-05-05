using Bogus;
using ByteSizeLib;
using Environment;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    #region Base

    private static Faker<T> ApplyDownloadTaskBase<T>(this Faker<T> faker, DownloadTaskType downloadTaskType)
        where T : DownloadTaskBase
    {
        return faker
            .StrictMode(true)
            .Ignore(x => x.Id)
            .RuleFor(x => x.Key, _ => GetUniqueNumber())
            .RuleFor(x => x.Title, f => f.PlexMedia().MediaTitle(downloadTaskType))
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
        DownloadTaskType downloadTaskType,
        Action<FakeDataConfig>? options = null
    )
        where T : DownloadTaskParentBase
    {
        var config = FakeDataConfig.FromOptions(options);

        return faker
            .ApplyDownloadTaskBase(downloadTaskType)
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
        DownloadTaskType downloadTaskType,
        Action<FakeDataConfig>? options = null
    )
        where T : DownloadTaskFileBase
    {
        var config = FakeDataConfig.FromOptions(options);

        return faker
            .ApplyDownloadTaskBase(downloadTaskType)
            .RuleFor(
                x => x.DataTotal,
                f =>
                    config.DownloadFileSizeInMb > 0
                        ? (long)ByteSize.FromMebiBytes(config.DownloadFileSizeInMb).Bytes
                        : f.Random.Long(1, 10000000)
            )
            .Ignore(x => x.DataReceived)
            .Ignore(x => x.DownloadSpeed)
            .Ignore(x => x.FileTransferSpeed)
            .Ignore(x => x.FileDataTransferred)
            .Ignore(x => x.CurrentFileTransferPathIndex)
            .Ignore(x => x.CurrentFileTransferBytesOffset)
            .Ignore(x => x.DestinationFolderPathId)
            .RuleFor(x => x.Quality, f => f.PickRandom("sd", "720p", "1080p", "2160p"))
            .RuleFor(
                x => x.FileName,
                (_, x) => $"{x.MediaType.ToPlexMediaTypeString()}-{x.Title.SanitizeFolderName()}.[{x.Quality}].file.mp4"
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

    private static readonly Faker<DownloadTaskMovie> _downloadTaskMovie = new Faker<DownloadTaskMovie>()
        .ApplyDownloadTaskParentBase(DownloadTaskType.Movie)
        .Ignore(x => x.Children)
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

    public static Faker<DownloadTaskMovie> GetMovieDownloadTask(Seed seed, Action<FakeDataConfig>? options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return _downloadTaskMovie
            .UseSeed(seed.Next())
            .RuleFor(
                x => x.Children,
                _ =>
                {
                    if (config.IncludeMultiPartMovies)
                        return GetDownloadTaskMovieFile(seed).Generate(2);

                    return GetDownloadTaskMovieFile(seed).Generate(1);
                }
            );
    }

    private static readonly Faker<DownloadTaskMovieFile> _downloadTaskMovieFile = new Faker<DownloadTaskMovieFile>()
        .ApplyDownloadTaskFileBase(DownloadTaskType.MovieData)
        .Ignore(x => x.Parent)
        .Ignore(x => x.ParentId);

    public static Faker<DownloadTaskMovieFile> GetDownloadTaskMovieFile(Seed seed) =>
        _downloadTaskMovieFile.UseSeed(seed.Next());

    #endregion

    #region TvShow

    private static readonly Faker<DownloadTaskTvShow> _downloadTaskTvShow = new Faker<DownloadTaskTvShow>()
        .ApplyDownloadTaskParentBase(DownloadTaskType.TvShow)
        .Ignore(x => x.Children)
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

    public static Faker<DownloadTaskTvShow> GetDownloadTaskTvShow(Seed seed, Action<FakeDataConfig>? options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return _downloadTaskTvShow
            .UseSeed(seed.Next())
            .RuleFor(
                x => x.Children,
                _ =>
                {
                    var f = GetDownloadTaskTvShowSeason(seed, options);
                    if (config.TvShowSeasonDownloadTasksCount > 0)
                        return f.Generate(config.TvShowSeasonDownloadTasksCount);

                    return f.GenerateBetween(1, 5);
                }
            );
    }

    private static readonly Faker<DownloadTaskTvShowSeason> _downloadTaskTvShowSeason =
        new Faker<DownloadTaskTvShowSeason>()
            .StrictMode(true)
            .ApplyDownloadTaskParentBase(DownloadTaskType.Season)
            .Ignore(x => x.ParentId)
            .RuleFor(x => x.Title, _ => "Season")
            .RuleFor(x => x.FullTitle, _ => "Season")
            .Ignore(x => x.Parent)
            .Ignore(x => x.Children)
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

    public static Faker<DownloadTaskTvShowSeason> GetDownloadTaskTvShowSeason(
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        return _downloadTaskTvShowSeason
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
            .UseSeed(seed.Next());
    }

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
        return _downloadTaskTvShowEpisode
            .UseSeed(seed.Next())
            .RuleFor(x => x.Children, _ => [GetDownloadTaskTvShowEpisodeFile(seed).Generate()]);
    }

    private static readonly Faker<DownloadTaskTvShowEpisodeFile> _downloadTaskTvShowEpisodeFile =
        new Faker<DownloadTaskTvShowEpisodeFile>()
            .StrictMode(true)
            .ApplyDownloadTaskFileBase(DownloadTaskType.EpisodeData)
            .Ignore(x => x.Parent)
            .Ignore(x => x.ParentId);

    public static Faker<DownloadTaskTvShowEpisodeFile> GetDownloadTaskTvShowEpisodeFile(Seed seed) =>
        _downloadTaskTvShowEpisodeFile.UseSeed(seed.Next());

    #endregion
}
