using Autofac.Features.Indexed;
using Quartz;

namespace Reaparr.Application.UnitTests;

public class DownloadJobUnitTests : BaseUnitTest<DownloadJob>
{
    private static IJobExecutionContext SetupJobContext(DownloadTaskKey key)
    {
        var jobDetail = new Mock<IJobDetail>();
        jobDetail
            .SetupGet(x => x.JobDataMap)
            .Returns(new DownloadJobPayload(key).ToJobDataMap());

        var context = new Mock<IJobExecutionContext>();
        context.SetupGet(x => x.JobDetail).Returns(jobDetail.Object);
        context.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        return context.Object;
    }

    [Test]
    public async Task ShouldSetDownloadAndDestinationPath_WhenDownloadTaskIsStarted()
    {
        // Arrange
        await SetupDatabase(
            39394,
            config =>
            {
                config.MovieDownloadTasksCount = 2;
            }
        );
        var testDownloadTask = IDbContext.DownloadTaskMovieFile.First();
        var expectedDestinationRootPath = testDownloadTask.DirectoryMeta.DestinationRootPath;
        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.DownloadSegments).Returns(4);
        var context = SetupJobContext(testDownloadTask.ToKey());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeterminePlexDownloadClientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(PlexDownloadClientType.Direct))
            .Verifiable(Times.Once());
        var downloadClientMock = Mock.Mock<IPlexDownloadClient>();
        downloadClientMock
            .Setup(x => x.Start(It.IsAny<DownloadTaskKey>(), CancellationToken))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());
        downloadClientMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable(Times.Once());

        var downloadClientIndexMock = new Mock<IIndex<PlexDownloadClientType, IPlexDownloadClient>>();
        downloadClientIndexMock.Setup(x => x[PlexDownloadClientType.Direct]).Returns(downloadClientMock.Object);

        var sut = Mock.Create<DownloadJob>(
            new NamedParameter("plexDownloadClientFactory", downloadClientIndexMock.Object)
        );

        // Act
        await sut.Execute(context);

        // Assert
        var downloadTaskResult = await IDbContext.DownloadTaskMovieFile.FirstOrDefaultAsync(
            x => x.Id == testDownloadTask.Id,
            CancellationToken
        );
        downloadTaskResult.ShouldNotBeNull();

        var downloadFolder = await IDbContext.GetDownloadFolder();
        await IDbContext.GetDefaultDestinationFolderPath(PlexMediaType.Movie);

        downloadTaskResult.DirectoryMeta.DownloadRootPath.ShouldBe(downloadFolder.DirectoryPath);
        downloadTaskResult.DirectoryMeta.DestinationRootPath.ShouldBe(expectedDestinationRootPath);
        downloadTaskResult.DownloadDirectory.ShouldContain(downloadFolder.DirectoryPath);
        downloadTaskResult.DestinationDirectory.ShouldContain(downloadTaskResult.DirectoryMeta.MovieFolder);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<DeterminePlexDownloadClientCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
        downloadClientMock.Verify(
            x => x.Start(It.Is<DownloadTaskKey>(key => key.Id == testDownloadTask.Id), It.IsAny<CancellationToken>()),
            Times.Once()
        );
        downloadClientMock.Verify(x => x.DisposeAsync(), Times.Once());
    }

    [Test]
    public async Task ShouldDisposeDownloadClient_WhenJobExecutionCompletes()
    {
        // Arrange
        await SetupDatabase(
            39395,
            config =>
            {
                config.MovieDownloadTasksCount = 1;
            }
        );

        var testDownloadTask = IDbContext.DownloadTaskMovieFile.First();
        var context = SetupJobContext(testDownloadTask.ToKey());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeterminePlexDownloadClientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(PlexDownloadClientType.Direct))
            .Verifiable(Times.Once());

        var downloadClientMock = Mock.Mock<IPlexDownloadClient>();
        downloadClientMock
            .Setup(x => x.Start(It.IsAny<DownloadTaskKey>(), CancellationToken))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());
        downloadClientMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var downloadClientIndexMock = new Mock<IIndex<PlexDownloadClientType, IPlexDownloadClient>>();
        downloadClientIndexMock.Setup(x => x[It.IsAny<PlexDownloadClientType>()]).Returns(downloadClientMock.Object);

        var sut = Mock.Create<DownloadJob>(
            new NamedParameter("plexDownloadClientFactory", downloadClientIndexMock.Object)
        );

        // Act
        await sut.Execute(context);

        // Assert
        downloadClientMock.Verify();
        downloadClientMock.Verify(
            x => x.Start(It.Is<DownloadTaskKey>(key => key.Id == testDownloadTask.Id), It.IsAny<CancellationToken>()),
            Times.Once()
        );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<DeterminePlexDownloadClientCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }

    [Test]
    public async Task ShouldSetSourceUnavailableStatus_WhenClientStartFailsWithNotFound()
    {
        // Arrange
        await SetupDatabase(
            39396,
            config =>
            {
                config.MovieDownloadTasksCount = 1;
            }
        );

        var testDownloadTask = IDbContext.DownloadTaskMovieFile.First();
        testDownloadTask.DownloadStatus = DownloadStatus.SourceUnavailable;
        await IDbContext.SaveChangesAsync(CancellationToken);

        var context = SetupJobContext(testDownloadTask.ToKey());

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeterminePlexDownloadClientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(PlexDownloadClientType.Direct))
            .Verifiable(Times.Once());
        Mock.Mock<IEventPublisher>()
            .Setup(x => x.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        var startResult = Result.Fail("Source is unavailable").Add404NotFoundError();
        var downloadClientMock = new Mock<IPlexDownloadClient>();
        downloadClientMock
            .Setup(x => x.Start(It.IsAny<DownloadTaskKey>(), CancellationToken))
            .ReturnsAsync(startResult)
            .Verifiable(Times.Once());
        downloadClientMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable(Times.Once());

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        var downloadClientIndexMock = new Mock<IIndex<PlexDownloadClientType, IPlexDownloadClient>>();
        downloadClientIndexMock.Setup(x => x[PlexDownloadClientType.Direct]).Returns(downloadClientMock.Object);

        var sut = Mock.Create<DownloadJob>(
            new NamedParameter("plexDownloadClientFactory", downloadClientIndexMock.Object)
        );

        // Act
        await sut.Execute(context);

        // Assert
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        DownloadStatus.SourceUnavailable,
                        It.IsAny<Result>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<DeterminePlexDownloadClientCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
        Mock.Mock<IEventPublisher>()
            .Verify(
                x => x.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
        downloadClientMock.Verify(
            x => x.Start(It.Is<DownloadTaskKey>(key => key.Id == testDownloadTask.Id), It.IsAny<CancellationToken>()),
            Times.Once()
        );
        downloadClientMock.Verify(x => x.DisposeAsync(), Times.Once());
    }

    [Test]
    public async Task ShouldCheckQueues_WhenDownloadFinishes()
    {
        // Arrange
        await SetupDatabase(
            39397,
            config =>
            {
                config.MovieDownloadTasksCount = 1;
            }
        );

        var testDownloadTask = IDbContext.DownloadTaskMovieFile.First();
        await IDbContext.SetDownloadStatus(testDownloadTask.ToKey(), DownloadStatus.DownloadFinished);
        var context = SetupJobContext(testDownloadTask.ToKey());

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeterminePlexDownloadClientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(PlexDownloadClientType.Direct));
        Mock.Mock<IMoveDownloadFileQueue>()
            .Setup(x => x.CheckMoveDownloadFileJobQueue(CancellationToken))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());
        Mock.Mock<IEventPublisher>()
            .Setup(x => x.PublishAsync(It.IsAny<CheckDownloadQueueEvent>(), CancellationToken))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        var downloadClientMock = new Mock<IPlexDownloadClient>();
        downloadClientMock
            .Setup(x => x.Start(It.IsAny<DownloadTaskKey>(), CancellationToken))
            .ReturnsAsync(Result.Ok());
        downloadClientMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var downloadClientIndexMock = new Mock<IIndex<PlexDownloadClientType, IPlexDownloadClient>>();
        downloadClientIndexMock.Setup(x => x[PlexDownloadClientType.Direct]).Returns(downloadClientMock.Object);
        var sut = Mock.Create<DownloadJob>(
            new NamedParameter("plexDownloadClientFactory", downloadClientIndexMock.Object)
        );

        // Act
        await sut.Execute(context);

        // Assert
        Mock.Mock<IMoveDownloadFileQueue>().Verify();
        Mock.Mock<IEventPublisher>().Verify();
    }

    [Test]
    public async Task ShouldSetDownloadClientErrorStatus_WhenClientStartFailsWithoutSpecificError()
    {
        // Arrange
        await SetupDatabase(
            39397,
            config =>
            {
                config.MovieDownloadTasksCount = 1;
            }
        );

        var testDownloadTask = IDbContext.DownloadTaskMovieFile.First();
        var context = SetupJobContext(testDownloadTask.ToKey());

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeterminePlexDownloadClientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(PlexDownloadClientType.Direct))
            .Verifiable(Times.Once());
        Mock.Mock<IEventPublisher>()
            .Setup(x => x.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        var startResult = Result.Fail("Client failed to start");
        var downloadClientMock = new Mock<IPlexDownloadClient>();
        downloadClientMock
            .Setup(x => x.Start(It.IsAny<DownloadTaskKey>(), CancellationToken))
            .ReturnsAsync(startResult)
            .Verifiable(Times.Once());
        downloadClientMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable(Times.Once());

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        var downloadClientIndexMock = new Mock<IIndex<PlexDownloadClientType, IPlexDownloadClient>>();
        downloadClientIndexMock.Setup(x => x[PlexDownloadClientType.Direct]).Returns(downloadClientMock.Object);

        var sut = Mock.Create<DownloadJob>(
            new NamedParameter("plexDownloadClientFactory", downloadClientIndexMock.Object)
        );

        // Act
        await sut.Execute(context);

        // Assert
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        DownloadStatus.DownloadClientError,
                        It.IsAny<Result>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<DeterminePlexDownloadClientCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
        Mock.Mock<IEventPublisher>()
            .Verify(
                x => x.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
        downloadClientMock.Verify(
            x => x.Start(It.Is<DownloadTaskKey>(key => key.Id == testDownloadTask.Id), It.IsAny<CancellationToken>()),
            Times.Once()
        );
        downloadClientMock.Verify(x => x.DisposeAsync(), Times.Once());
    }
}
