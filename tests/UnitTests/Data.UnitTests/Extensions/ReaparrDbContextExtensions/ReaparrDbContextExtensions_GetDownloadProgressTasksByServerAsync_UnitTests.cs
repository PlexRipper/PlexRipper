using Reaparr.Data.Contracts;

namespace Reaparr.Data.UnitTests;

public class ReaparrDbContextExtensionsGetDownloadProgressTasksByServerAsyncUnitTests : BaseUnitTest
{
    public ReaparrDbContextExtensionsGetDownloadProgressTasksByServerAsyncUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldPreserveFullTitles_WhenRetrievingDownloadProgressTasks()
    {
        // Arrange
        await SetupDatabase(
            129887,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieDownloadTasksCount = 3;
                config.TvShowDownloadTasksCount = 2;
                config.TvShowSeasonDownloadTasksCount = 2;
                config.TvShowEpisodeDownloadTasksCount = 2;
            }
        );

        // Act
        var progressTasks = await IDbContext.GetDownloadProgressTasksByServerAsync(0, CancellationToken);

        // Assert
        progressTasks.ShouldNotBeEmpty();
        progressTasks.Flatten(x => x.Children).ShouldAllBe(x => !string.IsNullOrWhiteSpace(x.FullTitle));
    }
}
