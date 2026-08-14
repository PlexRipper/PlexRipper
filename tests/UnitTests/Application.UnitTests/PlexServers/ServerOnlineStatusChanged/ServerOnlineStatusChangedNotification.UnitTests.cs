namespace Reaparr.Application.UnitTests;

public class ServerOnlineStatusChangedHandlerUnitTests : BaseUnitTest<ServerOnlineStatusChangedHandler>
{
    [Test]
    public async Task ShouldInvalidateServerLibraries_WhenServerGoesOffline()
    {
        // Arrange
        var mediaQueryCache = new Mock<IMediaQueryCache>(MockBehavior.Strict);
        SetupDependencies(builder =>
            builder.RegisterInstance(mediaQueryCache.Object).As<IMediaQueryCache>().SingleInstance()
        );

        await SetupDatabase(
            91301,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 2;
                config.PlexTvShowLibraryCount = 2;
            }
        );

        var dbContext = IDbContext;
        var targetServer = await dbContext.PlexServers.OrderBy(x => x.Id).FirstAsync(CancellationToken);
        var expectedLibraryIds = await dbContext
            .PlexLibraries.Where(x => x.PlexServerId == targetServer.Id)
            .Select(x => x.Id)
            .OrderBy(x => x)
            .ToListAsync(CancellationToken);

        mediaQueryCache
            .Setup(x =>
                x.InvalidateLibraries(
                    It.Is<IReadOnlyCollection<int>>(libraryIds =>
                        libraryIds.OrderBy(id => id).SequenceEqual(expectedLibraryIds)
                    ),
                    "Plex server online status changed"
                )
            )
            .Verifiable(Times.Once);

        Mock.Mock<IDownloadQueue>()
            .Setup(x => x.CheckDownloadQueue(It.IsAny<List<int>>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ResetFailedLibrarySyncJobsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);

        // Act
        await Sut.HandleAsync(new ServerOnlineStatusChangedNotification(targetServer.Id, false), CancellationToken);

        // Assert
        mediaQueryCache.Verify();
        Mock.Mock<IDownloadQueue>().Verify();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldInvalidateServerLibrariesAndResumeQueues_WhenServerComesOnline()
    {
        // Arrange
        var mediaQueryCache = new Mock<IMediaQueryCache>(MockBehavior.Strict);
        SetupDependencies(builder =>
            builder.RegisterInstance(mediaQueryCache.Object).As<IMediaQueryCache>().SingleInstance()
        );

        await SetupDatabase(
            91302,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 2;
                config.PlexTvShowLibraryCount = 2;
            }
        );

        var dbContext = IDbContext;
        var targetServer = await dbContext.PlexServers.OrderBy(x => x.Id).FirstAsync(CancellationToken);
        var expectedLibraryIds = await dbContext
            .PlexLibraries.Where(x => x.PlexServerId == targetServer.Id)
            .Select(x => x.Id)
            .OrderBy(x => x)
            .ToListAsync(CancellationToken);

        mediaQueryCache
            .Setup(x =>
                x.InvalidateLibraries(
                    It.Is<IReadOnlyCollection<int>>(libraryIds =>
                        libraryIds.OrderBy(id => id).SequenceEqual(expectedLibraryIds)
                    ),
                    "Plex server online status changed"
                )
            )
            .Verifiable(Times.Once);

        Mock.Mock<IDownloadQueue>()
            .Setup(x =>
                x.CheckDownloadQueue(
                    It.Is<List<int>>(serverIds => serverIds.SequenceEqual(new[] { targetServer.Id })),
                    CancellationToken
                )
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<ResetFailedLibrarySyncJobsCommand>(command => command.PlexServerId == targetServer.Id),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        // Act
        await Sut.HandleAsync(new ServerOnlineStatusChangedNotification(targetServer.Id, true), CancellationToken);

        // Assert
        mediaQueryCache.Verify();
        Mock.Mock<IDownloadQueue>().Verify();
        Mock.Mock<ICommandExecutor>().Verify();
    }
}
