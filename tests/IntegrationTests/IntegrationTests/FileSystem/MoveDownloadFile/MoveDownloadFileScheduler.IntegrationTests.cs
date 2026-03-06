using System.IO.Abstractions;
using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;

namespace Reaparr.IntegrationTests.FileSystem;

public class MoveDownloadFileSchedulerIntegrationTests : BaseIntegrationTests
{
    public MoveDownloadFileSchedulerIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldFinishMergingDownloadTaskAsFileTaskJobAndSetToCompleted_WhenDownloadTaskHasFinishedDownloading()
    {
        // Arrange
        using var container = await CreateContainer(
            235689,
            config =>
            {
                config.DatabaseOptions = x =>
                {
                    x.PlexServerCount = 1;
                    x.PlexMovieLibraryCount = 3;
                    x.MovieCount = 1;
                    x.MovieDownloadTasksCount = 1;
                    x.DownloadFileSizeInMb = 10;
                };

                config.FileSystemOptions = (system, dbContext) =>
                {
                    var downloadTask = dbContext.DownloadTaskMovieFile.First();
                    downloadTask.DownloadFilePath.ShouldNotBeNullOrEmpty();

                    // We need a DownloadFinished file to move
                    var directoryPath = system.Path.GetDirectoryName(downloadTask.DownloadFilePath);
                    directoryPath.ShouldNotBeNullOrEmpty();
                    system.Directory.CreateDirectory(directoryPath);
                    system.File.WriteAllBytes(downloadTask.DownloadFilePath, FakeData.GetDownloadFile(10.0 / 4.0));
                };
            }
        );
        var dbContext = container.DbContext;
        var downloadTasks = dbContext.DownloadTaskMovie.AsTracking().Include(x => x.Children).ToList();
        downloadTasks.ShouldNotBeNull();

        downloadTasks.SetDownloadStatus(DownloadStatus.DownloadFinished);
        await dbContext.SaveChangesAsync(CancellationToken);

        var downloadTask = container.DbContext.DownloadTaskMovieFile.First();
        var expectedSourcePath = downloadTask.DownloadFilePath;
        var expectedDestinationPath = downloadTask.DestinationFilePath;

        // Act
        var startResult = await container.MoveDownloadFileScheduler.StartMoveDownloadFileJob(downloadTask.ToKey());
        await container.SchedulerService.AwaitScheduler(CancellationToken);

        // Assert
        startResult.IsSuccess.ShouldBeTrue();

        var downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(downloadTask.ToKey(), CancellationToken);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);

        container.MockDownloadHubService.ServerDownloadProgressList.Count.ShouldBeGreaterThanOrEqualTo(3);

        var fileSystem = container.Resolve<IFileSystem>();
        fileSystem
            .File.Exists(expectedSourcePath)
            .ShouldBeFalse("Source .reaptemp file should have been removed after move");
        fileSystem.File.Exists(expectedDestinationPath).ShouldBeTrue("Destination file should exist after move");
    }
}
