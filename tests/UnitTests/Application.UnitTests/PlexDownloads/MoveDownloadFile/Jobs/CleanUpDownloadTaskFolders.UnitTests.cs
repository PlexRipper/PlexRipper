using System.IO.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Reaparr.Application.UnitTests;

public class CleanUpDownloadTaskFoldersUnitTests : BaseUnitTest<CleanUpDownloadTaskFoldersHandler>
{
    [Test]
    public async Task ShouldReturnSuccessResult_WhenDirectoryDoesNotExist()
    {
        // Arrange
        await SetupDatabase(
            25,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadTask.ShouldNotBeNull();

        Mock.Mock<IPath>()
            .Setup(x => x.GetDirectoryName(It.IsAny<string>()))
            .Returns("/mnt/DATA/ReaparrCache/Downloads/TvShows/Reno 911!/Season 1/")
            .Verifiable(Times.Exactly(2));

        Mock.Mock<IDirectory>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false).Verifiable(Times.Exactly(2));

        Mock.Mock<IDirectory>().Setup(x => x.GetFileSystemEntries(It.IsAny<string>())).Verifiable(Times.Never);

        Mock.Mock<IDirectory>().Setup(x => x.Delete(It.IsAny<string>())).Verifiable(Times.Never);

        // Act
        var request = new CleanUpDownloadTaskFoldersCommand(downloadTask.ToKey());
        var result = await Sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IPath>().Verify();
        Mock.Mock<IDirectory>().Verify();
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenFilePathIsEmpty()
    {
        // Arrange
        await SetupDatabase(
            25,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadTask.ShouldNotBeNull();

        // Act
        var request = new CleanUpDownloadTaskFoldersCommand(downloadTask.ToKey());
        var result = await Sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailed.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnSuccessResult_WhenFilePathIsValid()
    {
        // Arrange
        await SetupDatabase(
            25,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadTask.ShouldNotBeNull();

        Mock.Mock<IPath>()
            .Setup(x => x.GetDirectoryName(It.IsAny<string>()))
            .Returns("/mnt/DATA/ReaparrCache/Downloads/TvShows/Reno 911!/Season 1/")
            .Verifiable(Times.Exactly(2));

        Mock.Mock<IPath>()
            .Setup(x => x.GetFileName(It.IsAny<string>()))
            .Returns("Reno 911! - S01E01 - How We Do It in Reno (Pilot) WEBDL-1080p.part1.mkv")
            .Verifiable(Times.Never);

        Mock.Mock<IDirectory>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true).Verifiable(Times.Exactly(2));
        Mock.Mock<IDirectory>()
            .Setup(x => x.GetFileSystemEntries(It.IsAny<string>()))
            .Returns([])
            .Verifiable(Times.Exactly(2));
        Mock.Mock<IDirectory>().Setup(x => x.Delete(It.IsAny<string>())).Verifiable(Times.Exactly(2));

        // Act
        var request = new CleanUpDownloadTaskFoldersCommand(downloadTask.ToKey());
        var result = await Sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IPath>().Verify();
        Mock.Mock<IDirectory>().Verify();
    }

    [Test]
    public async Task ShouldReturnSuccessResult_WhenDirectoryContainsEntries()
    {
        // Arrange
        await SetupDatabase(
            25,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadTask.ShouldNotBeNull();

        Mock.Mock<IPath>()
            .Setup(x => x.GetDirectoryName(It.IsAny<string>()))
            .Returns("/mnt/DATA/ReaparrCache/Downloads/TvShows/Reno 911!/Season 1/")
            .Verifiable(Times.Exactly(2));

        Mock.Mock<IPath>()
            .Setup(x => x.GetFileName(It.IsAny<string>()))
            .Returns("file.mkv")
            .Verifiable(Times.Exactly(4));

        Mock.Mock<IDirectory>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true).Verifiable(Times.Exactly(2));

        Mock.Mock<IDirectory>()
            .Setup(x => x.GetFileSystemEntries(It.IsAny<string>()))
            .Returns([
                "/mnt/DATA/ReaparrCache/Downloads/TvShows/Reno 911!/Season 1/Reno 911! - S01E01.mkv",
                "/mnt/DATA/ReaparrCache/Downloads/TvShows/Reno 911!/Season 1/Reno 911! - S01E02.mkv",
            ])
            .Verifiable(Times.Exactly(2));

        Mock.Mock<IDirectory>().Setup(x => x.Delete(It.IsAny<string>())).Verifiable(Times.Never);

        // Act
        var request = new CleanUpDownloadTaskFoldersCommand(downloadTask.ToKey());
        var result = await Sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IPath>().Verify();
        Mock.Mock<IDirectory>().Verify();
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenUnauthorizedAccessExceptionIsThrown()
    {
        // Arrange
        await SetupDatabase(
            25,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadTask.ShouldNotBeNull();

        Mock.Mock<IPath>()
            .Setup(x => x.GetDirectoryName(It.IsAny<string>()))
            .Returns("/mnt/DATA/ReaparrCache/Downloads/TvShows/Reno 911!/Season 1/")
            .Verifiable(Times.Once);

        Mock.Mock<IDirectory>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true).Verifiable(Times.Once);

        Mock.Mock<IDirectory>()
            .Setup(x => x.GetFileSystemEntries(It.IsAny<string>()))
            .Throws(new UnauthorizedAccessException())
            .Verifiable(Times.Once);

        // Act
        var request = new CleanUpDownloadTaskFoldersCommand(downloadTask.ToKey());
        var result = await Sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailed.ShouldBeTrue();
        result.HasException<UnauthorizedAccessException>().ShouldBeTrue();
        Mock.Mock<IPath>().Verify();
        Mock.Mock<IDirectory>().Verify();
    }

    [Test]
    public async Task ShouldSkipDirectoryCleanup_WhenAnotherTaskInSameDirectoryIsStillActive()
    {
        // Arrange
        await SetupDatabase(
            25001,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 2;
            }
        );

        var dbContext = IDbContext;
        var fileTasks = await dbContext.DownloadTaskTvShowEpisodeFile.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        fileTasks.Count.ShouldBeGreaterThanOrEqualTo(2);

        var completedTask = fileTasks[0];
        var activeSiblingTask = fileTasks[1];

        completedTask.DownloadStatus = DownloadStatus.Completed;
        activeSiblingTask.DownloadStatus = DownloadStatus.Queued;
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var request = new CleanUpDownloadTaskFoldersCommand(completedTask.ToKey());
        var result = await Sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IPath>().Verify(x => x.GetDirectoryName(It.IsAny<string>()), Times.Never);
        Mock.Mock<IDirectory>().Verify(x => x.GetFileSystemEntries(It.IsAny<string>()), Times.Never);
        Mock.Mock<IDirectory>().Verify(x => x.Delete(It.IsAny<string>()), Times.Never);
    }
}
