using ByteSizeLib;
using Serilog.Events;

namespace Reaparr.BaseTests.UnitTests;

public class FakeDataDownloadTasksUnitTests : BaseUnitTest
{
    public FakeDataDownloadTasksUnitTests(ITestOutputHelper output)
        : base(output, LogEventLevel.Information) { }

    [Fact]
    public void MovieDownloadTask_ShouldGenerateAllRequiredProperties()
    {
        // Arrange
        var seed = new Seed(12345);

        // Act
        var movieTask = FakeData.GetMovieDownloadTask(seed).Generate();

        // Assert
        movieTask.ShouldNotBeNull();
        movieTask.PlexId.ShouldBeGreaterThan(0);
        movieTask.Title.ShouldNotBeNullOrEmpty();
        movieTask.FullTitle.ShouldNotBeNullOrEmpty();
        movieTask.FullTitle.ShouldContain("Movie");
        movieTask.DownloadStatus.ShouldBe(DownloadStatus.Queued);
        movieTask.CreatedAt.ShouldNotBe(default);
        movieTask.Year.ShouldBeInRange(1900, 2030);
        movieTask.DataTotal.ShouldBeGreaterThan(0);
        movieTask.MediaType.ShouldBe(PlexMediaType.Movie);
        movieTask.DownloadTaskType.ShouldBe(DownloadTaskType.Movie);
        movieTask.PlexServerId.ShouldBe(0); // Should be 0 since it's ignored
        movieTask.PlexLibraryId.ShouldBe(0); // Should be 0 since it's ignored
    }

    [Fact]
    public void MovieDownloadTask_ShouldGenerateChildren()
    {
        // Arrange
        var seed = new Seed(12345);
        var config = new FakeDataConfig { IncludeMultiPartMovies = false };

        // Act
        var movieTask = FakeData
            .GetMovieDownloadTask(seed, options => options.IncludeMultiPartMovies = config.IncludeMultiPartMovies)
            .Generate();

        // Assert
        movieTask.Children.ShouldNotBeNull();
        movieTask.Children.Count.ShouldBe(1);
    }

    [Fact]
    public void MovieDownloadTask_ShouldGenerateMultipleChildrenWhenConfigured()
    {
        // Arrange
        var seed = new Seed(12345);
        var config = new FakeDataConfig { IncludeMultiPartMovies = true };

        // Act
        var movieTask = FakeData
            .GetMovieDownloadTask(seed, options => options.IncludeMultiPartMovies = config.IncludeMultiPartMovies)
            .Generate();

        // Assert
        movieTask.Children.ShouldNotBeNull();
        movieTask.Children.Count.ShouldBe(2);
    }

    [Fact]
    public void MovieFileDownloadTask_ShouldGenerateAllRequiredProperties()
    {
        // Arrange
        var seed = new Seed(12345);

        // Act
        var movieFileTask = FakeData.GetDownloadTaskMovieFile(seed).Generate();

        // Assert
        movieFileTask.ShouldNotBeNull();
        movieFileTask.PlexId.ShouldBeGreaterThan(0);
        movieFileTask.Title.ShouldNotBeNullOrEmpty();
        movieFileTask.FullTitle.ShouldNotBeNullOrEmpty();
        movieFileTask.DownloadStatus.ShouldBe(DownloadStatus.Queued);
        movieFileTask.CreatedAt.ShouldNotBe(default);
        movieFileTask.FileName.ShouldNotBeNullOrEmpty();
        movieFileTask.FileLocationUrl.ShouldNotBeNullOrEmpty();
        movieFileTask.Quality.ShouldNotBeNullOrEmpty();
        movieFileTask.DataTotal.ShouldBeGreaterThan(0);
        movieFileTask.DirectoryMeta.ShouldNotBeNull();
        movieFileTask.DirectoryMeta.DownloadRootPath.ShouldNotBeNullOrEmpty();
        movieFileTask.DirectoryMeta.DestinationRootPath.ShouldNotBeNullOrEmpty();
        movieFileTask.DirectoryMeta.MovieFolder.ShouldNotBeNullOrEmpty();
        movieFileTask.MediaType.ShouldBe(PlexMediaType.Movie);
        movieFileTask.DownloadTaskType.ShouldBe(DownloadTaskType.MovieData);
    }

    [Fact]
    public void TvShowDownloadTask_ShouldGenerateAllRequiredProperties()
    {
        // Arrange
        var seed = new Seed(67890);

        // Act
        var tvShowTask = FakeData.GetDownloadTaskTvShow(seed).Generate();

        // Assert
        tvShowTask.ShouldNotBeNull();
        tvShowTask.PlexId.ShouldBeGreaterThan(0);
        tvShowTask.Title.ShouldNotBeNullOrEmpty();
        tvShowTask.FullTitle.ShouldNotBeNullOrEmpty();
        tvShowTask.DownloadStatus.ShouldBe(DownloadStatus.Queued);
        tvShowTask.CreatedAt.ShouldNotBe(default);
        tvShowTask.Year.ShouldBeInRange(1900, 2030);
        tvShowTask.DataTotal.ShouldBeGreaterThan(0);
        tvShowTask.MediaType.ShouldBe(PlexMediaType.TvShow);
        tvShowTask.DownloadTaskType.ShouldBe(DownloadTaskType.TvShow);
    }

    [Fact]
    public void TvShowDownloadTask_ShouldGenerateChildrenWithCorrectHierarchy()
    {
        // Arrange
        var seed = new Seed(67890);
        var config = new FakeDataConfig
        {
            TvShowSeasonDownloadTasksCount = 2,
            TvShowEpisodeDownloadTasksCount = 3,
            DownloadWorkerTasks = 4,
        };

        // Act
        var tvShowTask = FakeData
            .GetDownloadTaskTvShow(
                seed,
                options =>
                {
                    options.TvShowSeasonDownloadTasksCount = config.TvShowSeasonDownloadTasksCount;
                    options.TvShowEpisodeDownloadTasksCount = config.TvShowEpisodeDownloadTasksCount;
                    options.DownloadWorkerTasks = config.DownloadWorkerTasks;
                }
            )
            .Generate();

        // Assert
        tvShowTask.Children.ShouldNotBeNull();
        tvShowTask.Children.Count.ShouldBe(config.TvShowSeasonDownloadTasksCount);

        foreach (var season in tvShowTask.Children)
        {
            season.ShouldNotBeNull();
            season.Children.Count.ShouldBe(config.TvShowEpisodeDownloadTasksCount);
            season.FullTitle.ShouldStartWith(tvShowTask.FullTitle);

            foreach (var episode in season.Children)
            {
                episode.ShouldNotBeNull();
                episode.Children.Count.ShouldBe(1);
                episode.FullTitle.ShouldStartWith(season.FullTitle);

                foreach (var episodeFile in episode.Children)
                {
                    episodeFile.ShouldNotBeNull();
                    episodeFile.FullTitle.ShouldStartWith(episode.FullTitle);
                    episodeFile.DirectoryMeta.ShouldNotBeNull();
                    episodeFile.DirectoryMeta.TvShowFolder.ShouldBe(tvShowTask.Title);
                    episodeFile.DirectoryMeta.SeasonFolder.ShouldBe(season.Title);

                    episodeFile.DownloadWorkerTasks.ShouldNotBeNull();
                    episodeFile.DownloadWorkerTasks.Count.ShouldBe(config.DownloadWorkerTasks);
                }
            }
        }
    }

    [Fact]
    public void DownloadTaskConfig_ShouldModifyFileSizeWhenConfigured()
    {
        // Arrange
        var seed = new Seed(12345);
        const int configuredSizeMb = 500;
        Action<FakeDataConfig> options = options =>
        {
            options.DownloadFileSizeInMb = configuredSizeMb;
        };

        // Act
        var movieFileTask = FakeData.GetDownloadTaskMovieFile(seed).Generate();
        var configuredMovieFileTask = FakeData.GetDownloadTaskMovieFile(seed, options).Generate();

        // Assert
        movieFileTask.DataTotal.ShouldNotBe(configuredMovieFileTask.DataTotal);
        var expectedBytes = ByteSize.FromMebiBytes(configuredSizeMb).Bytes;
        configuredMovieFileTask.DataTotal.ShouldBe((long)expectedBytes);
    }

    [Fact]
    public void AllDownloadTaskTypes_ShouldGenerateCorrectTypes()
    {
        // Arrange
        var seed = new Seed(24680);

        // Act & Assert
        var movieTask = FakeData.GetMovieDownloadTask(seed).Generate();
        movieTask.DownloadTaskType.ShouldBe(DownloadTaskType.Movie);

        var movieFileTask = FakeData.GetDownloadTaskMovieFile(seed).Generate();
        movieFileTask.DownloadTaskType.ShouldBe(DownloadTaskType.MovieData);

        var tvShowTask = FakeData.GetDownloadTaskTvShow(seed).Generate();
        tvShowTask.DownloadTaskType.ShouldBe(DownloadTaskType.TvShow);

        var seasonTask = FakeData.GetDownloadTaskTvShowSeason(seed).Generate();
        seasonTask.DownloadTaskType.ShouldBe(DownloadTaskType.Season);

        var episodeTask = FakeData.GetDownloadTaskTvShowEpisode(seed).Generate();
        episodeTask.DownloadTaskType.ShouldBe(DownloadTaskType.Episode);

        var episodeFileTask = FakeData.GetDownloadTaskTvShowEpisodeFile(seed).Generate();
        episodeFileTask.DownloadTaskType.ShouldBe(DownloadTaskType.EpisodeData);
    }
}
