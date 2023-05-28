using Data.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data;
using PlexRipper.Data.Common;

namespace Data.UnitTests.Commands;

public class UpdateRootDownloadStatusOfDownloadTaskCommandHandler_UnitTests : BaseUnitTest
{
    public UpdateRootDownloadStatusOfDownloadTaskCommandHandler_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldAllBeQueuedDownloadTasks_WhenAllChildrenAreQueuedStatus()
    {
        // Arrange
        Seed = 9679;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowCount = 5;
            config.TvShowDownloadTasksCount = 1;
        });

        var downloadTasks = await DbContext.DownloadTasks.IncludeDownloadTasks().IncludeByRoot().ToListAsync();
        var request = new UpdateRootDownloadStatusOfDownloadTaskCommand(downloadTasks.First().Id);
        var handler = new UpdateRootDownloadStatusOfDownloadTaskCommandHandler(_log, GetDbContext());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var flattenDownloadTasks = downloadTasks.Flatten(x => x.Children).ToList();
        var downloadTasksDb = await DbContext.DownloadTasks.ToListAsync();
        downloadTasksDb.Count.ShouldBe(flattenDownloadTasks.Count);
        downloadTasksDb.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Queued);
    }

    [Fact]
    public async Task ShouldAllBeCompletedDownloadTasks_WhenOnlyAllChildrenAreCompletedStatus()
    {
        // Arrange
        Seed = 95299;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowCount = 5;
            config.TvShowDownloadTasksCount = 1;
        });

        var downloadTasks = await DbContext.DownloadTasks
            .IncludeDownloadTasks()
            .IncludeByRoot()
            .ToListAsync();

        foreach (var seasonDownloadTask in downloadTasks[0].Children)
        foreach (var episodeDownloadTask in seasonDownloadTask.Children)
            episodeDownloadTask.Children = episodeDownloadTask.Children.SetToCompleted();
        SaveChanges();

        var request = new UpdateRootDownloadStatusOfDownloadTaskCommand(downloadTasks.First().Id);
        var handler = new UpdateRootDownloadStatusOfDownloadTaskCommandHandler(_log, GetDbContext());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var flattenDownloadTasks = downloadTasks.Flatten(x => x.Children).ToList();
        var downloadTasksDb = await DbContext.DownloadTasks.ToListAsync();
        downloadTasksDb.Count.ShouldBe(flattenDownloadTasks.Count);
        downloadTasksDb.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Completed);
    }

    [Fact]
    public async Task ShouldBeInErrorStatus_WhenOneChildHasErrorStatus()
    {
        // Arrange
        Seed = 9999;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowCount = 5;
            config.TvShowDownloadTasksCount = 1;
        });

        var downloadTasks = await DbContext.DownloadTasks.IncludeDownloadTasks().IncludeByRoot().ToListAsync();
        downloadTasks[0].Children[0].Children[0].Children[0].DownloadStatus = DownloadStatus.Error;
        SaveChanges();
        var request = new UpdateRootDownloadStatusOfDownloadTaskCommand(downloadTasks.First().Id);
        var handler = new UpdateRootDownloadStatusOfDownloadTaskCommandHandler(_log, GetDbContext());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        ResetDbContext();
        result.IsSuccess.ShouldBeTrue();
        var flattenDownloadTasks = downloadTasks.Flatten(x => x.Children).ToList();
        var downloadTasksDb = await DbContext.DownloadTasks.ToListAsync();
        downloadTasksDb.Count.ShouldBe(flattenDownloadTasks.Count);
        var downloadTaskDb = DbContext.DownloadTasks.IncludeDownloadTasks().FirstOrDefault(x => x.Id == 1);
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Error);
        downloadTaskDb.Children[0].DownloadStatus.ShouldBe(DownloadStatus.Error);
        downloadTaskDb.Children[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Error);
        downloadTaskDb.Children[0].Children[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Error);
        downloadTaskDb.Children[1].DownloadStatus.ShouldBe(DownloadStatus.Queued);
        downloadTaskDb.Children[1].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Queued);
        downloadTaskDb.Children[1].Children[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Queued);
    }
}