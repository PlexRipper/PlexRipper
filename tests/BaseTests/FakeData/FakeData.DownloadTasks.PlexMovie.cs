using ByteSizeLib;
using Reaparr.Application;

namespace Reaparr.BaseTests;

public static partial class FakeData
{
    private static Faker<DownloadTaskMovie> CreateDownloadTaskMovieFaker()
    {
        return new Faker<DownloadTaskMovie>()
            .ApplyDownloadTaskParentBase(DownloadTaskType.Movie)
            .Ignore(x => x.Children)
            .FinishWith(
                (_, movie) =>
                {
                    var movieIndex = 1;
                    foreach (var movieFile in movie.Children)
                    {
                        var currentMovieIndex = movieIndex++;
                        movieFile.Title = $"{movieFile.Title} {currentMovieIndex}";
                        movieFile.FullTitle = $"{movie.FullTitle}/{currentMovieIndex}-{movieFile.FileName}";
                    }
                }
            );
    }

    public static Faker<DownloadTaskMovie> GetMovieDownloadTask(Seed seed, Action<FakeDataConfig>? options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return CreateDownloadTaskMovieFaker()
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

    private static Faker<DownloadTaskMovieFile> CreateDownloadTaskMovieFileFaker()
    {
        return new Faker<DownloadTaskMovieFile>()
            .ApplyDownloadTaskFileBase(DownloadTaskType.MovieData)
            .Ignore(x => x.Parent)
            .Ignore(x => x.ParentId)
            .Ignore(x => x.Logs);
    }

    public static Faker<DownloadTaskMovieFile> GetDownloadTaskMovieFile(
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        return CreateDownloadTaskMovieFileFaker()
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
