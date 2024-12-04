using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.EntityFrameworkCore;
using MockFileSystem = System.IO.Abstractions.TestingHelpers.MockFileSystem;

namespace PlexRipper.Application.UnitTests;

public class CreateDirectoryFromFilePathUnitTests : BaseUnitTest<CleanUpDownloadTaskFoldersHandler>
{
    public CreateDirectoryFromFilePathUnitTests(ITestOutputHelper output)
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
            .FirstOrDefaultAsync();
        downloadTask.ShouldNotBeNull();

        var downloadWorkerTaskIds = downloadTask.DownloadWorkerTasks.Select(x => x.Id).ToList();
        await dbContext
            .DownloadWorkerTasks.Where(x => downloadWorkerTaskIds.Contains(x.Id))
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadDirectory, "").SetProperty(x => x.FileName, ""));

        // Act
        var request = new CleanUpDownloadTaskFoldersCommand(downloadTask.ToKey());
        var result = await _sut.Handle(request, CancellationToken.None);

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
            .FirstOrDefaultAsync();
        downloadTask.ShouldNotBeNull();

        var downloadWorkerTaskIds = downloadTask.DownloadWorkerTasks.Select(x => x.Id).ToList();
        await dbContext
            .DownloadWorkerTasks.Where(x => downloadWorkerTaskIds.Contains(x.Id))
            .ExecuteUpdateAsync(p =>
                p.SetProperty(
                        x => x.DownloadDirectory,
                        "/mnt/DATA/PlexRipperCache/Downloads/TvShows/Reno 911!/Season 1"
                    )
                    .SetProperty(
                        x => x.FileName,
                        "Reno 911! - S01E01 - How We Do It in Reno (Pilot) WEBDL-1080p.part1.mkv"
                    )
            );

        mock.Mock<IPath>()
            .Setup(x => x.GetDirectoryName(It.IsAny<string>()))
            .Returns("/mnt/DATA/PlexRipperCache/Downloads/TvShows/Reno 911!/Season 1/");

        mock.Mock<IDirectory>().Setup(x => x.GetFiles(It.IsAny<string>())).Returns([]);
        mock.Mock<IDirectory>().Setup(x => x.Delete(It.IsAny<string>()));

        // Act
        var request = new CleanUpDownloadTaskFoldersCommand(downloadTask.ToKey());
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
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
            .FirstOrDefaultAsync();
        downloadTask.ShouldNotBeNull();

        var downloadWorkerTaskIds = downloadTask.DownloadWorkerTasks.Select(x => x.Id).ToList();
        await dbContext
            .DownloadWorkerTasks.Where(x => downloadWorkerTaskIds.Contains(x.Id))
            .ExecuteUpdateAsync(p =>
                p.SetProperty(
                        x => x.DownloadDirectory,
                        "/mnt/DATA/PlexRipperCache/Downloads/TvShows/Reno 911!/Season 1"
                    )
                    .SetProperty(
                        x => x.FileName,
                        "Reno 911! - S01E01 - How We Do It in Reno (Pilot) WEBDL-1080p.part1.mkv"
                    )
            );

        mock.Mock<IPath>()
            .Setup(x => x.GetDirectoryName(It.IsAny<string>()))
            .Returns("/mnt/DATA/PlexRipperCache/Downloads/TvShows/Reno 911!/Season 1/")
            .Verifiable(Times.Once);

        mock.Mock<IDirectory>()
            .Setup(x => x.GetFiles(It.IsAny<string>()))
            .Throws(new UnauthorizedAccessException())
            .Verifiable(Times.Once);

        // Act
        var request = new CleanUpDownloadTaskFoldersCommand(downloadTask.ToKey());
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailed.ShouldBeTrue();
        result.HasException<UnauthorizedAccessException>().ShouldBeTrue();
    }
}
