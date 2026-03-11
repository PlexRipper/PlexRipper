using System.IO.Abstractions;
using System.Reactive.Linq;
using Autofac;
using FastEndpoints;
using Reaparr.Data.Contracts;
using Reaparr.External.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application.UnitTests;

public class DashPlexDownloadClientDisposeAsyncUnitTests : BaseUnitTest<DashPlexDownloadClient>
{
    public DashPlexDownloadClientDisposeAsyncUnitTests(ITestOutputHelper output)
        : base(output) { }

    private DashPlexDownloadClient CreateSut(Mock<IDashMpdCliWrapper>? wrapperMock = null)
    {
        var dashWrapperMock = wrapperMock ?? new Mock<IDashMpdCliWrapper>();
        var directoryMock = new Mock<IDirectory>();

        directoryMock.Setup(x => x.CreateDirectory(It.IsAny<string>()));
        dashWrapperMock.Setup(x => x.Progress).Returns(Observable.Empty<DashDownloadProgress>());
        dashWrapperMock.Setup(x => x.StandardOutput).Returns(Observable.Empty<string>());
        dashWrapperMock.Setup(x => x.DownloadCompleted).Returns(Observable.Empty<DashDownloadCompletedEventArgs>());
        dashWrapperMock.Setup(x => x.StartAsync(It.IsAny<DashMpdCliOptions>())).ReturnsAsync(Result.Ok());
        dashWrapperMock.Setup(x => x.StopAsync()).ReturnsAsync(Result.Ok());
        dashWrapperMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result<GetTranscodeUrlResult>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Ok(
                    new GetTranscodeUrlResult
                    {
                        DownloadUrl = "https://plex.example/start.mpd",
                        TranscodedQuality = VideoQuality.FullHD,
                    }
                )
            );

        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        return Mock.Create<DashPlexDownloadClient>(
            new NamedParameter("dashWrapper", dashWrapperMock.Object),
            new NamedParameter("directory", directoryMock.Object)
        );
    }

    [Fact]
    public async Task ShouldDisposeWithoutError_WhenDisposeAsyncIsCalledWithoutPriorStart()
    {
        var dashWrapperMock = new Mock<IDashMpdCliWrapper>();
        dashWrapperMock.Setup(x => x.Progress).Returns(Observable.Empty<DashDownloadProgress>());
        dashWrapperMock.Setup(x => x.StandardOutput).Returns(Observable.Empty<string>());
        dashWrapperMock.Setup(x => x.DownloadCompleted).Returns(Observable.Empty<DashDownloadCompletedEventArgs>());
        dashWrapperMock.Setup(x => x.StopAsync()).ReturnsAsync(Result.Ok());
        dashWrapperMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var sut = CreateSut(dashWrapperMock);

        var act = async () => await sut.DisposeAsync();

        await act.ShouldNotThrowAsync();
        dashWrapperMock.Verify(x => x.DisposeAsync(), Times.Once);
        dashWrapperMock.Verify(x => x.StopAsync(), Times.Once);
    }

    [Fact]
    public async Task ShouldBeIdempotent_WhenDisposeAsyncIsCalledMultipleTimes()
    {
        var dashWrapperMock = new Mock<IDashMpdCliWrapper>();
        dashWrapperMock.Setup(x => x.Progress).Returns(Observable.Empty<DashDownloadProgress>());
        dashWrapperMock.Setup(x => x.StandardOutput).Returns(Observable.Empty<string>());
        dashWrapperMock.Setup(x => x.DownloadCompleted).Returns(Observable.Empty<DashDownloadCompletedEventArgs>());
        dashWrapperMock.Setup(x => x.StartAsync(It.IsAny<DashMpdCliOptions>())).ReturnsAsync(Result.Ok());
        dashWrapperMock.Setup(x => x.StopAsync()).ReturnsAsync(Result.Ok());
        dashWrapperMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var sut = CreateSut(dashWrapperMock);

        await sut.DisposeAsync();
        await sut.DisposeAsync();

        dashWrapperMock.Verify(x => x.StopAsync(), Times.Once);
        dashWrapperMock.Verify(x => x.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task ShouldDisposeDbContext_WhenDisposeAsyncIsCalled()
    {
        var dbContextMock = new Mock<IReaparrDbContext>();
        dbContextMock.Setup(x => x.Dispose()).Verifiable(Times.Once);

        var dbContextFactoryMock = new Mock<IReaparrDbContextFactory>();
        dbContextFactoryMock.Setup(x => x.Create()).Returns(dbContextMock.Object);

        var dashWrapperMock = new Mock<IDashMpdCliWrapper>();
        dashWrapperMock.Setup(x => x.Progress).Returns(Observable.Empty<DashDownloadProgress>());
        dashWrapperMock.Setup(x => x.StandardOutput).Returns(Observable.Empty<string>());
        dashWrapperMock.Setup(x => x.DownloadCompleted).Returns(Observable.Empty<DashDownloadCompletedEventArgs>());
        dashWrapperMock.Setup(x => x.StopAsync()).ReturnsAsync(Result.Ok());
        dashWrapperMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var sut = Mock.Create<DashPlexDownloadClient>(
            new NamedParameter("dbContextFactory", dbContextFactoryMock.Object),
            new NamedParameter("dashWrapper", dashWrapperMock.Object)
        );

        await sut.DisposeAsync();

        dbContextFactoryMock.Verify(x => x.Create(), Times.Once);
        dbContextMock.Verify();
        dashWrapperMock.Verify(x => x.StopAsync(), Times.Once);
        dashWrapperMock.Verify(x => x.DisposeAsync(), Times.Once);
    }
}
