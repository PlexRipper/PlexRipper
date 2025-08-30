using Microsoft.EntityFrameworkCore;
using Reaparr.BaseTests;

namespace Reaparr.Application.UnitTests.ClearCompleted;

public class ClearCompletedDownloadTasksEndpoint_UnitTests : BaseUnitTest<ClearCompletedDownloadTasksEndpoint>
{
    public ClearCompletedDownloadTasksEndpoint_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldRemoveOnlySpecifiedCompletedDownloadTasks_WhenClearCompletedEndpointIsCalledWithGuidList()
    {
        // Arrange
        await SetupDatabase(
            213132,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieDownloadTasksCount = 10;
            }
        );

        // Set download tasks to completed
        var dbContext = IDbContext;
        var downloadTasks = await dbContext
            .DownloadTaskMovie.AsTracking()
            .Include(x => x.Children)
            .ToListAsync(CancellationToken);

        downloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var ep = SetupEndpointUnitTest<ClearCompletedDownloadTasksEndpoint>();
        await ep.HandleAsync(downloadTasks.Select(x => x.Id).Take(5).ToList(), CancellationToken);
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        var downloadTasksDb = await IDbContext.DownloadTaskMovie.ToListAsync(CancellationToken);
        var downloadTasksFileDb = await IDbContext.DownloadTaskMovieFile.ToListAsync(CancellationToken);
        downloadTasksDb.Count.ShouldBe(5);
        downloadTasksFileDb.Count.ShouldBe(5);
    }

    [Fact]
    public async Task ShouldRemoveAllCompletedDownloadTasks_WhenClearCompletedEndpointIsCalled()
    {
        // Arrange
        await SetupDatabase(
            34036,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieDownloadTasksCount = 10;
            }
        );

        // Set download tasks to completed
        var dbContext = IDbContext;
        var downloadTasks = await dbContext
            .DownloadTaskMovie.AsTracking()
            .Include(x => x.Children)
            .ToListAsync(CancellationToken);

        downloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var ep = SetupEndpointUnitTest<ClearCompletedDownloadTasksEndpoint>();
        await ep.HandleAsync([], CancellationToken);
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        dbContext = IDbContext;

        var downloadTasksDb = await dbContext.DownloadTaskMovie.ToListAsync(CancellationToken);
        var downloadTasksFileDb = await dbContext.DownloadTaskMovieFile.ToListAsync(CancellationToken);
        downloadTasksDb.ShouldBeEmpty();
        downloadTasksFileDb.ShouldBeEmpty();
    }
}
