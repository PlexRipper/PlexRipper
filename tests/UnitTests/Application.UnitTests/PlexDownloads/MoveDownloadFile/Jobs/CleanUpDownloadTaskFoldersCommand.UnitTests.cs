using System.IO.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Reaparr.Application.UnitTests;

public class CleanUpDownloadTaskFoldersCommandUnitTests : BaseUnitTest<CleanUpDownloadTaskFoldersHandler>
{
    public CleanUpDownloadTaskFoldersCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
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
                config.DownloadWorkerTasks = 4;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext
            .DownloadTaskTvShowEpisodeFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(CancellationToken);
        downloadTask.ShouldNotBeNull();

        var downloadWorkerTaskIds = downloadTask.DownloadWorkerTasks.Select(x => x.Id).ToList();
        await dbContext
            .DownloadWorkerTasks.Where(x => downloadWorkerTaskIds.Contains(x.Id))
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadDirectory, "").SetProperty(x => x.FileName, ""),
                CancellationToken
            );

        // Act
        var request = new CleanUpDownloadTaskFoldersCommand(downloadTask.ToKey());
        var result = await Sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
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
                config.DownloadWorkerTasks = 4;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext
            .DownloadTaskTvShowEpisodeFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(CancellationToken);
        downloadTask.ShouldNotBeNull();

        var downloadWorkerTaskIds = downloadTask.DownloadWorkerTasks.Select(x => x.Id).ToList();
        await dbContext
            .DownloadWorkerTasks.Where(x => downloadWorkerTaskIds.Contains(x.Id))
            .ExecuteUpdateAsync(
                p =>
                    p.SetProperty(
                            x => x.DownloadDirectory,
                            "/mnt/DATA/ReaparrCache/Downloads/TvShows/Reno 911!/Season 1"
                        )
                        .SetProperty(
                            x => x.FileName,
                            "Reno 911! - S01E01 - How We Do It in Reno (Pilot) WEBDL-1080p.part1.mkv"
                        ),
                CancellationToken
            );

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

    [Fact]
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
                config.DownloadWorkerTasks = 4;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext
            .DownloadTaskTvShowEpisodeFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(CancellationToken);
        downloadTask.ShouldNotBeNull();

        var downloadWorkerTaskIds = downloadTask.DownloadWorkerTasks.Select(x => x.Id).ToList();
        await dbContext
            .DownloadWorkerTasks.Where(x => downloadWorkerTaskIds.Contains(x.Id))
            .ExecuteUpdateAsync(
                p =>
                    p.SetProperty(
                            x => x.DownloadDirectory,
                            "/mnt/DATA/ReaparrCache/Downloads/TvShows/Reno 911!/Season 1"
                        )
                        .SetProperty(
                            x => x.FileName,
                            "Reno 911! - S01E01 - How We Do It in Reno (Pilot) WEBDL-1080p.part1.mkv"
                        ),
                CancellationToken
            );

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
            .Returns(
                [
                    "/mnt/DATA/ReaparrCache/Downloads/TvShows/Reno 911!/Season 1/Reno 911! - S01E01.mkv",
                    "/mnt/DATA/ReaparrCache/Downloads/TvShows/Reno 911!/Season 1/Reno 911! - S01E02.mkv",
                ]
            )
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

    [Fact]
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
                config.DownloadWorkerTasks = 4;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext
            .DownloadTaskTvShowEpisodeFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(CancellationToken);
        downloadTask.ShouldNotBeNull();
        downloadTask.DownloadWorkerTasks.ShouldNotBeEmpty();

        var downloadWorkerTaskIds = downloadTask.DownloadWorkerTasks.Select(x => x.Id).ToList();
        await dbContext
            .DownloadWorkerTasks.Where(x => downloadWorkerTaskIds.Contains(x.Id))
            .ExecuteUpdateAsync(
                p =>
                    p.SetProperty(
                            x => x.DownloadDirectory,
                            "/mnt/DATA/ReaparrCache/Downloads/TvShows/Reno 911!/Season 1"
                        )
                        .SetProperty(
                            x => x.FileName,
                            "Reno 911! - S01E01 - How We Do It in Reno (Pilot) WEBDL-1080p.part1.mkv"
                        ),
                CancellationToken
            );

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
}
