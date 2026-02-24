using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;

namespace Reaparr.PublicAPI.UnitTests;

public class DeleteTorrentEndpointUnitTests : BaseUnitTest
{
    public DeleteTorrentEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldDeleteDownloadTask_WhenHashIdMatches()
    {
        // Arrange
        await SetupDatabase(
            1234,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var movieFile = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var hashId = "abc123";
        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                x => x.SetProperty(p => p.HashId, hashId).SetProperty(p => p.DownloadStatus, DownloadStatus.Completed),
                CancellationToken
            );

        var request = new DeleteTorrentRequest { Hashes = [hashId] };

        // Act
        var endpoint = SetupEndpointUnitTest<DeleteTorrentEndpoint>();
        await endpoint.HandleAsync(request, CancellationToken);

        // Assert
        endpoint.HttpContext.Response.StatusCode.ShouldBe(200);
        var deletedMovieFile = await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .SingleOrDefaultAsync(CancellationToken);
        deletedMovieFile.ShouldBeNull();
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<StopDownloadTaskCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ShouldStopDownloadTask_WhenHashIdMatchesAndDownloading()
    {
        // Arrange
        await SetupDatabase(
            5678,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var movieFile = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var hashId = "def456";
        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                x => x.SetProperty(p => p.HashId, hashId).SetProperty(p => p.DownloadStatus, DownloadStatus.Downloading),
                CancellationToken
            );

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<StopDownloadTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var request = new DeleteTorrentRequest { Hashes = [hashId] };

        // Act
        var endpoint = SetupEndpointUnitTest<DeleteTorrentEndpoint>();
        await endpoint.HandleAsync(request, CancellationToken);

        // Assert
        endpoint.HttpContext.Response.StatusCode.ShouldBe(200);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<StopDownloadTaskCommand>(cmd => cmd.DownloadTaskGuid == movieFile.Id),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        var deletedMovieFile = await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .SingleOrDefaultAsync(CancellationToken);
        deletedMovieFile.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldStopDownloadTask_WhenHashIdMatchesHashesRawAndDownloading()
    {
        // Arrange
        await SetupDatabase(
            9012,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var movieFile = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var hashId = "d42bd234e00c24b9e10f20f61e1a32d10c7efde9";
        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                x => x.SetProperty(p => p.HashId, hashId).SetProperty(p => p.DownloadStatus, DownloadStatus.Downloading),
                CancellationToken
            );

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<StopDownloadTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var request = new DeleteTorrentRequest { DeleteFiles = true, Hashes = null, HashesRaw = hashId };

        // Act
        var endpoint = SetupEndpointUnitTest<DeleteTorrentEndpoint>();
        await endpoint.HandleAsync(request, CancellationToken);

        // Assert
        endpoint.HttpContext.Response.StatusCode.ShouldBe(200);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<StopDownloadTaskCommand>(cmd => cmd.DownloadTaskGuid == movieFile.Id),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        var deletedMovieFile = await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .SingleOrDefaultAsync(CancellationToken);
        deletedMovieFile.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldStopDownloadTask_WhenHashIdCasingDiffersInHashesRaw()
    {
        // Arrange
        await SetupDatabase(
            3456,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var movieFile = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var storedHashId = "D42BD234E00C24B9E10F20F61E1A32D10C7EFDE9";
        var requestHashId = "d42bd234e00c24b9e10f20f61e1a32d10c7efde9";
        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                x =>
                    x.SetProperty(p => p.HashId, storedHashId)
                        .SetProperty(p => p.DownloadStatus, DownloadStatus.Downloading),
                CancellationToken
            );

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<StopDownloadTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var request = new DeleteTorrentRequest { HashesRaw = requestHashId };

        // Act
        var endpoint = SetupEndpointUnitTest<DeleteTorrentEndpoint>();
        await endpoint.HandleAsync(request, CancellationToken);

        // Assert
        endpoint.HttpContext.Response.StatusCode.ShouldBe(200);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<StopDownloadTaskCommand>(cmd => cmd.DownloadTaskGuid == movieFile.Id),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }
}
