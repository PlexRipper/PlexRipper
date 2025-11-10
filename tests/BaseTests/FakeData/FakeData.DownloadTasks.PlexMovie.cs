using ByteSizeLib;
using Reaparr.Application;

namespace Reaparr.BaseTests;

public static partial class FakeData
{
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
                x => x.DataTotal,
                (_, x) =>
                    config.DownloadFileSizeInMb > 0
                        ? (long)ByteSize.FromMebiBytes(config.DownloadFileSizeInMb).Bytes
                        : x.DataTotal
            )
            .RuleFor(
                x => x.Children,
                _ => GetDownloadTaskMovieFile(seed, options).Generate(config.IncludeMultiPartMovies ? 2 : 1)
            );
    }

    private static readonly Faker<DownloadTaskMovieFile> _downloadTaskMovieFile = new Faker<DownloadTaskMovieFile>()
        .ApplyDownloadTaskFileBase(DownloadTaskType.MovieData)
        .Ignore(x => x.Parent)
        .Ignore(x => x.ParentId);

    public static Faker<DownloadTaskMovieFile> GetDownloadTaskMovieFile(
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        return _downloadTaskMovieFile
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
