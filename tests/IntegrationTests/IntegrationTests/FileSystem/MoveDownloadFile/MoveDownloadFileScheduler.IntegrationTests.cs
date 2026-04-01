using System.IO.Abstractions;
using Autofac;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application;
using Reaparr.Data.Contracts;

namespace Reaparr.IntegrationTests;

public class MoveDownloadFileSchedulerIntegrationTests : BaseIntegrationTests
{
    [Test]
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
        var expectedDownloadFolderFinalNamePath = expectedSourcePath.RemoveReapTempSuffix();
        var expectedDestinationPath = downloadTask.DestinationFilePath;

        // Act
        var startResult = await container.MoveDownloadFileScheduler.StartMoveDownloadFileJob(downloadTask.ToKey());
        await container.SchedulerService.AwaitScheduler(CancellationToken);

        // Assert
        startResult.IsSuccess.ShouldBeTrue();

        var downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(downloadTask.ToKey(), CancellationToken);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);

        var patchDeadline = DateTime.UtcNow.AddSeconds(5);
        var completedPatchReceived = false;
        while (DateTime.UtcNow < patchDeadline)
        {
            if (container.MockDownloadHubService.DownloadPatchList.TryTake(out var patch, 100, CancellationToken))
            {
                completedPatchReceived = patch.Upserts.Any(x =>
                    x.Id == downloadTask.Id && x.Status == DownloadStatus.Completed
                );
                if (completedPatchReceived)
                    break;
            }
        }

        completedPatchReceived.ShouldBeTrue();

        var fileSystem = container.Resolve<IFileSystem>();
        fileSystem
            .File.Exists(expectedSourcePath)
            .ShouldBeFalse("Source .reaptemp file should have been removed after move");
        fileSystem
            .File.Exists(expectedDownloadFolderFinalNamePath)
            .ShouldBeFalse("Download folder should not keep a duplicate final media file");
        fileSystem.File.Exists(expectedDestinationPath).ShouldBeTrue("Destination file should exist after move");
    }

    [Test]
    public async Task ShouldSetMoveErrorAndKeepSourceFile_WhenMoveFailsAfterCopy()
    {
        // Arrange
        using var container = await CreateContainer(
            235690,
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

                    var directoryPath = system.Path.GetDirectoryName(downloadTask.DownloadFilePath);
                    directoryPath.ShouldNotBeNullOrEmpty();
                    system.Directory.CreateDirectory(directoryPath);
                    system.File.WriteAllBytes(downloadTask.DownloadFilePath, FakeData.GetDownloadFile(10.0 / 4.0));
                };

                config.OverrideServices = builder =>
                    builder
                        .Register(ctx =>
                        {
                            var file = ctx.Resolve<IFile>();

                            return new FakeCommandExecutor().Intercept<MoveFileWithResumeCommand, Result>(
                                async (moveCmd, ct) =>
                                {
                                    await using var writeStream = file.Open(
                                        moveCmd.TargetPath,
                                        FileMode.OpenOrCreate,
                                        FileAccess.Write,
                                        FileShare.ReadWrite
                                    );
                                    await using var readStream = file.Open(
                                        moveCmd.SourcePath,
                                        FileMode.Open,
                                        FileAccess.Read,
                                        FileShare.ReadWrite
                                    );

                                    await readStream.CopyToAsync(writeStream, ct);

                                    return Result.Fail(
                                        $"Failed to delete source file after move: {moveCmd.SourcePath}"
                                    );
                                }
                            );
                        })
                        .As<ICommandExecutor>()
                        .InstancePerDependency();
            }
        );

        var downloadTasks = container.DbContext.DownloadTaskMovie.AsTracking().Include(x => x.Children).ToList();
        downloadTasks.ShouldNotBeNull();

        downloadTasks.SetDownloadStatus(DownloadStatus.DownloadFinished);
        await container.DbContext.SaveChangesAsync(CancellationToken);

        var downloadTask = container.DbContext.DownloadTaskMovieFile.First();
        var sourcePath = downloadTask.DownloadFilePath;
        var destinationPath = downloadTask.DestinationFilePath;

        // Act
        var startResult = await container.MoveDownloadFileScheduler.StartMoveDownloadFileJob(downloadTask.ToKey());
        await container.SchedulerService.AwaitScheduler(CancellationToken);

        // Assert
        startResult.IsSuccess.ShouldBeTrue();

        var downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(downloadTask.ToKey(), CancellationToken);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.MoveError);

        var fileSystem = container.Resolve<IFileSystem>();
        fileSystem.File.Exists(sourcePath).ShouldBeTrue("Source file should remain when delete step fails");
        fileSystem.File.Exists(destinationPath).ShouldBeTrue("Destination file should still be present after copy");
    }
}
