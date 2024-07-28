using Data.Contracts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace PlexRipper.Application.UnitTests.ClearCompleted;

public class ClearCompletedDownloadTasksEndpoint_UnitTests : BaseUnitTest<ClearCompletedDownloadTasksEndpoint>
{
    public ClearCompletedDownloadTasksEndpoint_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldRemoveOnlySpecifiedCompletedDownloadTasks_WhenClearCompletedEndpointIsCalledWithGuidList()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieDownloadTasksCount = 10;
        });

        // Set download tasks to completed
        var dbContext = IDbContext;
        var downloadTasks = await IDbContext.DownloadTaskMovie.AsTracking().Include(x => x.Children).ToListAsync();

        downloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        await IDbContext.SaveChangesAsync(CancellationToken.None);

        var ep = Factory.Create<ClearCompletedDownloadTasksEndpoint>(ctx =>
            ctx.AddTestServices(s => s.AddTransient(_ => mock.Create<IPlexRipperDbContext>()))
        );

        // Act
        await ep.HandleAsync(downloadTasks.Select(x => x.Id).Take(5).ToList(), default);
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        var downloadTasksDb = await IDbContext.DownloadTaskMovie.ToListAsync();
        var downloadTasksFileDb = await IDbContext.DownloadTaskMovieFile.ToListAsync();
        downloadTasksDb.Count.ShouldBe(5);
        downloadTasksFileDb.Count.ShouldBe(5);
    }

    [Fact]
    public async Task ShouldRemoveAllCompletedDownloadTasks_WhenClearCompletedEndpointIsCalled()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieDownloadTasksCount = 10;
        });

        // Set download tasks to completed
        var dbContext = IDbContext;
        var downloadTasks = await IDbContext.DownloadTaskMovie.AsTracking().Include(x => x.Children).ToListAsync();

        downloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        await IDbContext.SaveChangesAsync(CancellationToken.None);

        var ep = Factory.Create<ClearCompletedDownloadTasksEndpoint>(ctx =>
            ctx.AddTestServices(s => s.AddTransient(_ => mock.Create<IPlexRipperDbContext>()))
        );

        // Act
        await ep.HandleAsync(new List<Guid>(), default);
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        var downloadTasksDb = await IDbContext.DownloadTaskMovie.ToListAsync();
        var downloadTasksFileDb = await IDbContext.DownloadTaskMovieFile.ToListAsync();
        downloadTasksDb.ShouldBeEmpty();
        downloadTasksFileDb.ShouldBeEmpty();
    }
}
