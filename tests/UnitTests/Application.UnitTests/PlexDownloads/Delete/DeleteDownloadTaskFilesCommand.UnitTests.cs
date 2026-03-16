using System.IO.Abstractions.TestingHelpers;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class DeleteDownloadTaskFilesCommandUnitTests : BaseUnitTest<DeleteDownloadTaskFilesCommandHandler>
{
    public DeleteDownloadTaskFilesCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldDeletePlainFileFromDownloadDirectory_WhenTaskIsCompleted()
    {
        // Arrange
        await SetupDatabase(
            84001,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );
        var dbContext = IDbContext;
        var movieFileTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var movieFileKey = await dbContext.DownloadTaskMovieFile.ProjectToKey().FirstAsync(CancellationToken);

        // Completed tasks have the plain file (no .reaptemp) in the download directory.
        var plainFilePath = movieFileTask.DownloadFilePath.RemoveReapTempSuffix();
        SetupFileSystem(fs => fs.AddFile(plainFilePath, new MockFileData([])));

        // Act
        var result = await Sut.ExecuteAsync(new DeleteDownloadTaskFilesCommand([movieFileKey]), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var file = Mock.Create<System.IO.Abstractions.IFile>();
        file.Exists(plainFilePath).ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldDeleteReapTempFileFromDownloadDirectory_WhenFileHasTempSuffix()
    {
        // Arrange
        await SetupDatabase(
            84002,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );
        var dbContext = IDbContext;
        var movieFileTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var movieFileKey = await dbContext.DownloadTaskMovieFile.ProjectToKey().FirstAsync(CancellationToken);

        // Active download: file still has .reaptemp suffix.
        var reapTempPath = movieFileTask.DownloadFilePath;
        SetupFileSystem(fs => fs.AddFile(reapTempPath, new MockFileData([])));

        // Act
        var result = await Sut.ExecuteAsync(new DeleteDownloadTaskFilesCommand([movieFileKey]), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var file = Mock.Create<System.IO.Abstractions.IFile>();
        file.Exists(reapTempPath).ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldSucceed_WhenFileIsNotPresentOnDisk()
    {
        // Arrange — no files on disk; handler should succeed gracefully.
        await SetupDatabase(
            84003,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );
        var movieFileKey = await IDbContext.DownloadTaskMovieFile.ProjectToKey().FirstAsync(CancellationToken);

        SetupFileSystem();

        // Act
        var result = await Sut.ExecuteAsync(new DeleteDownloadTaskFilesCommand([movieFileKey]), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldDeleteFiles_WhenMultipleKeysAreGiven()
    {
        // Arrange
        await SetupDatabase(
            84004,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 2;
                config.MovieDownloadTasksCount = 2;
            }
        );
        var dbContext = IDbContext;
        var movieFileTasks = await dbContext.DownloadTaskMovieFile.ToListAsync(CancellationToken);
        movieFileTasks.Count.ShouldBe(2);
        var movieFileKeys = await dbContext.DownloadTaskMovieFile.ProjectToKey().ToListAsync(CancellationToken);

        var plainPaths = movieFileTasks.Select(t => t.DownloadFilePath.RemoveReapTempSuffix()).ToList();
        SetupFileSystem(fs =>
        {
            foreach (var path in plainPaths)
                fs.AddFile(path, new MockFileData([]));
        });

        // Act
        var result = await Sut.ExecuteAsync(new DeleteDownloadTaskFilesCommand(movieFileKeys), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var file = Mock.Create<System.IO.Abstractions.IFile>();
        foreach (var path in plainPaths)
            file.Exists(path).ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldDeleteEmptyDirectoryAfterFileDeletion_WhenDirectoryBecomesEmpty()
    {
        // Arrange
        await SetupDatabase(
            84005,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );
        var dbContext = IDbContext;
        var movieFileTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var movieFileKey = await dbContext.DownloadTaskMovieFile.ProjectToKey().FirstAsync(CancellationToken);
        var plainFilePath = movieFileTask.DownloadFilePath.RemoveReapTempSuffix();

        SetupFileSystem(fs => fs.AddFile(plainFilePath, new MockFileData([])));

        // Act
        var result = await Sut.ExecuteAsync(new DeleteDownloadTaskFilesCommand([movieFileKey]), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var directory = Mock.Create<System.IO.Abstractions.IDirectory>();
        directory.Exists(movieFileTask.DownloadDirectory).ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldNotDeleteDirectory_WhenOtherFilesRemainInIt()
    {
        // Arrange — place an unrelated sibling file in the same download directory so it stays
        // non-empty after the task's own file is deleted.
        await SetupDatabase(
            84006,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );
        var dbContext = IDbContext;
        var movieFileTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var movieFileKey = await dbContext.DownloadTaskMovieFile.ProjectToKey().FirstAsync(CancellationToken);

        var pathToDelete = movieFileTask.DownloadFilePath.RemoveReapTempSuffix();
        // Sibling file that is not owned by this task — directory must survive.
        var siblingPath = System.IO.Path.Combine(movieFileTask.DownloadDirectory, "sibling.mkv");

        SetupFileSystem(fs =>
        {
            fs.AddFile(pathToDelete, new MockFileData([]));
            fs.AddFile(siblingPath, new MockFileData([]));
        });

        // Act — only delete the task's file; sibling remains
        var result = await Sut.ExecuteAsync(new DeleteDownloadTaskFilesCommand([movieFileKey]), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var file = Mock.Create<System.IO.Abstractions.IFile>();
        file.Exists(pathToDelete).ShouldBeFalse();
        file.Exists(siblingPath).ShouldBeTrue();

        // Directory still exists because siblingPath is still there.
        var directory = Mock.Create<System.IO.Abstractions.IDirectory>();
        directory.Exists(movieFileTask.DownloadDirectory).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldHandleEpisodeFileKeys()
    {
        // Arrange — TV show episode file task
        await SetupDatabase(
            84007,
            config =>
            {
                config.PlexServerCount = 1;
                config.TvShowCount = 1;
                config.TvShowDownloadTasksCount = 1;
            }
        );
        var dbContext = IDbContext;
        var episodeFileTask = await dbContext.DownloadTaskTvShowEpisodeFile.FirstAsync(CancellationToken);
        var episodeFileKey = await dbContext.DownloadTaskTvShowEpisodeFile.ProjectToKey().FirstAsync(CancellationToken);
        var plainFilePath = episodeFileTask.DownloadFilePath.RemoveReapTempSuffix();

        SetupFileSystem(fs => fs.AddFile(plainFilePath, new MockFileData([])));

        // Act
        var result = await Sut.ExecuteAsync(new DeleteDownloadTaskFilesCommand([episodeFileKey]), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var file = Mock.Create<System.IO.Abstractions.IFile>();
        file.Exists(plainFilePath).ShouldBeFalse();
    }
}
