using Data.Contracts;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.DownloadManager;

namespace DownloadManager.UnitTests;

public class DownloadTaskFactory_GenerateMovieDownloadTasksAsync_UnitTests : BaseUnitTest<DownloadTaskFactory>
{
    public DownloadTaskFactory_GenerateMovieDownloadTasksAsync_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenPlexMoviesAreEmpty()
    {
        // Arrange
        var movies = new List<int>();

        // Act
        var result = await _sut.GenerateMovieDownloadTasksAsync(movies);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldHaveValidSingleNestedDownloadTasks_WhenPlexMoviesAreValid()
    {
        // Arrange
        Seed = 324;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 5;
            config.IncludeMultiPartMovies = false;
        });

        var movies = DbContext.PlexMovies.IncludePlexLibrary().IncludePlexServer().ToList();

        mock.SetupMediator(It.IsAny<GetMultiplePlexMoviesByIdsQuery>)
            .ReturnsAsync(
                (GetMultiplePlexMoviesByIdsQuery query, CancellationToken _) => Result.Ok(movies.Where(x => query.Ids.Contains(x.Id)).ToList())
            );

        // Act
        var result = await _sut.GenerateMovieDownloadTasksAsync(movies.Select(x => x.Id).ToList());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(5);

        for (var i = 0; i < movies.Count; i++)
        {
            var plexMovie = movies[i];
            var downloadTask = result.Value[i];
            var fullTitle = $"{plexMovie.Title} ({plexMovie.Year})";

            downloadTask.Key.ShouldBe(plexMovie.Key);
            downloadTask.Title.ShouldBe(plexMovie.Title);
            downloadTask.FullTitle.ShouldBe(fullTitle);
            downloadTask.DataTotal.ShouldBe(plexMovie.MediaSize);
            downloadTask.Year.ShouldBe(plexMovie.Year);

            downloadTask.PlexLibrary.ShouldNotBeNull();
            downloadTask.PlexLibraryId.ShouldBeGreaterThan(0);
            downloadTask.PlexServer.ShouldNotBeNull();
            downloadTask.PlexServerId.ShouldBeGreaterThan(0);

            downloadTask.MediaType.ShouldBe(plexMovie.Type);
            downloadTask.DownloadTaskType.ShouldBe(DownloadTaskType.MovieData);
            downloadTask.DownloadStatus.ShouldBe(DownloadStatus.Queued);
            downloadTask.Created.ShouldBeGreaterThan(DateTime.MinValue);
            downloadTask.Created.ShouldBeLessThan(DateTime.UtcNow);

            downloadTask.Children.ShouldBeEmpty();
        }
    }
}