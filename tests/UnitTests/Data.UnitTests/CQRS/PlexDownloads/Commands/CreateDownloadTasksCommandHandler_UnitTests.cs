using Data.Contracts;
using DownloadManager.Contracts;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace Data.UnitTests.Commands;

public class CreateDownloadTasksCommandHandler_UnitTests : BaseUnitTest<CreateDownloadTasksCommandHandler>
{
    public CreateDownloadTasksCommandHandler_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldCreateAllDownloadTasks_WhenAllAreNew()
    {
        // Arrange
        await SetupDatabase(config => config.DisableForeignKeyCheck = true);
        var downloadTasks = FakeData.GetTvShowDownloadTask(options: config =>
        {
            config.MovieDownloadTasksCount = 5;
            config.TvShowDownloadTasksCount = 5;
            config.TvShowSeasonDownloadTasksCount = 5;
            config.TvShowEpisodeDownloadTasksCount = 5;
        }).Generate(1);

        mock.Mock<IDownloadTaskFactory>().Setup(x => x.GenerateAsync(It.IsAny<List<DownloadMediaDTO>>())).ReturnsAsync(Result.Ok(downloadTasks));
        mock.Mock<IDownloadTaskValidator>().Setup(x => x.ValidateDownloadTasks(It.IsAny<List<DownloadTask>>())).Returns(Result.Ok(downloadTasks));
        mock.Mock<IPlexRipperDbContext>()
            .Setup(x => x.BulkInsertAsync(It.IsAny<List<DownloadTask>>(), It.IsAny<BulkConfig>(), It.IsAny<CancellationToken>()))
            .Returns<List<DownloadTask>, BulkConfig, CancellationToken>((tasks, bulkConfig, token) => DbContext.BulkInsertAsync(tasks, bulkConfig, token));
        mock.Mock<IMediator>().Setup(x => x.Publish(It.IsAny<CheckDownloadQueue>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var request = new CreateDownloadTasksCommand(null);
        var handler = mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var flattenDownloadTasks = downloadTasks.Flatten(x => x.Children).ToList();
        DbContext.DownloadTasks.Count().ShouldBe(flattenDownloadTasks.Count);
    }

    [Fact]
    public async Task ShouldCreateOnlyChildDownloadTasks_WhenParentAlreadyExists()
    {
        // Arrange
        await SetupDatabase(config => config.DisableForeignKeyCheck = true);
        var downloadTasks = FakeData.GetTvShowDownloadTask().Generate(1);
        await DbContext.BulkInsertAsync(new List<DownloadTask> { downloadTasks.First() });
        downloadTasks[0].Id = 1;

        mock.Mock<IDownloadTaskFactory>().Setup(x => x.GenerateAsync(It.IsAny<List<DownloadMediaDTO>>())).ReturnsAsync(Result.Ok(downloadTasks));
        mock.Mock<IDownloadTaskValidator>().Setup(x => x.ValidateDownloadTasks(It.IsAny<List<DownloadTask>>())).Returns(Result.Ok(downloadTasks));
        mock.Mock<IPlexRipperDbContext>()
            .Setup(x => x.BulkInsertAsync(It.IsAny<List<DownloadTask>>(), It.IsAny<BulkConfig>(), It.IsAny<CancellationToken>()))
            .Returns<List<DownloadTask>, BulkConfig, CancellationToken>((tasks, bulkConfig, token) => DbContext.BulkInsertAsync(tasks, bulkConfig, token));
        mock.Mock<IMediator>().Setup(x => x.Publish(It.IsAny<CheckDownloadQueue>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var request = new CreateDownloadTasksCommand(null);
        var handler = mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var flattenDownloadTasks = downloadTasks.Flatten(x => x.Children).ToList();
        DbContext.DownloadTasks.Count().ShouldBe(flattenDownloadTasks.Count);
    }

    [Fact]
    public async Task ShouldAllHaveARootDownloadTaskId_WhenDownloadTasksAreChildren()
    {
        // Arrange
        await SetupDatabase(config => config.DisableForeignKeyCheck = true);
        var downloadTasks = FakeData.GetTvShowDownloadTask().Generate(1);

        mock.Mock<IDownloadTaskFactory>().Setup(x => x.GenerateAsync(It.IsAny<List<DownloadMediaDTO>>())).ReturnsAsync(Result.Ok(downloadTasks));
        mock.Mock<IDownloadTaskValidator>().Setup(x => x.ValidateDownloadTasks(It.IsAny<List<DownloadTask>>())).Returns(Result.Ok(downloadTasks));
        mock.Mock<IPlexRipperDbContext>()
            .Setup(x => x.BulkInsertAsync(It.IsAny<List<DownloadTask>>(), It.IsAny<BulkConfig>(), It.IsAny<CancellationToken>()))
            .Returns<List<DownloadTask>, BulkConfig, CancellationToken>((tasks, bulkConfig, token) => DbContext.BulkInsertAsync(tasks, bulkConfig, token));
        mock.Mock<IMediator>().Setup(x => x.Publish(It.IsAny<CheckDownloadQueue>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var request = new CreateDownloadTasksCommand(null);
        var handler = mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var downloadTasksDb = await DbContext.DownloadTasks.IncludeDownloadTasks().IncludeByRoot().ToListAsync();

        void HasRootDownloadTaskId(List<DownloadTask> downloadTasks)
        {
            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.RootDownloadTaskId.ShouldBe(1);
                if (downloadTask.Children.Any())
                    HasRootDownloadTaskId(downloadTask.Children);
            }
        }

        HasRootDownloadTaskId(downloadTasksDb.SelectMany(x => x.Children).ToList());
    }
}