namespace PlexRipper.Application.UnitTests;

public class GetAllDownloadTasksQueryHandler_UnitTests : BaseUnitTest<GetAllDownloadTasksQueryHandler>
{
    public GetAllDownloadTasksQueryHandler_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldReturnNoDownloadTasks_WhenNoDownloadTasksAreInDb()
    {
        // Arrange
        await SetupDatabase();
        var request = new GetAllDownloadTasksQuery();
        var handler = mock.Create<GetAllDownloadTasksQueryHandler>();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldReturnMovieDownloadTasks_WhenMovieDownloadTasksAreInDB()
    {
        // Arrange
        Seed = 21467;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 2;
            config.MovieCount = 20;
            config.MovieDownloadTasksCount = 10;
        });

        var request = new GetAllDownloadTasksQuery();
        var handler = mock.Create<GetAllDownloadTasksQueryHandler>();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var downloadTasks = result.Value;
        downloadTasks.ShouldNotBeEmpty();
        downloadTasks.Count.ShouldBe(10);

        var flatList = downloadTasks.Flatten(x => x.Children).ToList();
        flatList.ShouldAllBe(x => x.PlexServer != null);
        flatList.ShouldAllBe(x => x.PlexLibrary != null);
        flatList.ShouldAllBe(x => x.DownloadFolder != null);
        flatList.ShouldAllBe(x => x.DestinationFolder != null);
    }

    [Fact]
    public async Task ShouldAllTvShowDownloadTasksWithAllIncludes_WhenTvShowDownloadTasksAreInDB()
    {
        // Arrange
        Seed = 2767;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 2;
            config.TvShowCount = 20;
            config.TvShowDownloadTasksCount = 5;
            config.TvShowSeasonDownloadTasksCount = 5;
            config.TvShowEpisodeDownloadTasksCount = 5;
        });

        var request = new GetAllDownloadTasksQuery();
        var handler = mock.Create<GetAllDownloadTasksQueryHandler>();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var downloadTasks = result.Value;
        downloadTasks.ShouldNotBeEmpty();

        void ValidateDownloadTasks(List<DownloadTaskGeneric> shouldDownloadTasks)
        {
            downloadTasks.Count.ShouldBe(5);
            foreach (var downloadTask in shouldDownloadTasks)
            {
                downloadTask.PlexServer.ShouldNotBeNull();
                downloadTask.PlexLibrary.ShouldNotBeNull();
                downloadTask.DownloadFolder.ShouldNotBeNull();
                downloadTask.DestinationFolder.ShouldNotBeNull();
                if (downloadTask.Children.Any())
                    ValidateDownloadTasks(downloadTask.Children);
            }
        }

        ValidateDownloadTasks(downloadTasks);
        downloadTasks.Flatten(x => x.Children).ToList().Count.ShouldBe(280);
    }
}