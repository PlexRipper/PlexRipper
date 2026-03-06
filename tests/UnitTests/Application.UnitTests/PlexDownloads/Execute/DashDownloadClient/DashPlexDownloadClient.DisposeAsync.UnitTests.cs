using System.IO.Abstractions;
using System.Reactive.Linq;
using Autofac;
using FastEndpoints;
using Reaparr.External.Contracts;

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
        dashWrapperMock.Setup(x => x.StartAsync(It.IsAny<DashMpdCliOptions>())).ReturnsAsync(Result.Ok());
        dashWrapperMock.Setup(x => x.StopAsync()).ReturnsAsync(Result.Ok());
        dashWrapperMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result<string>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok("https://plex.example/start.mpd"));

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
        var sut = CreateSut();

        var act = async () => await sut.DisposeAsync();

        await act.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task ShouldBeIdempotent_WhenDisposeAsyncIsCalledMultipleTimes()
    {
        var dashWrapperMock = new Mock<IDashMpdCliWrapper>();
        dashWrapperMock.Setup(x => x.Progress).Returns(Observable.Empty<DashDownloadProgress>());
        dashWrapperMock.Setup(x => x.StandardOutput).Returns(Observable.Empty<string>());
        dashWrapperMock.Setup(x => x.StartAsync(It.IsAny<DashMpdCliOptions>())).ReturnsAsync(Result.Ok());
        dashWrapperMock.Setup(x => x.StopAsync()).ReturnsAsync(Result.Ok());
        dashWrapperMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var sut = CreateSut(dashWrapperMock);

        await sut.DisposeAsync();
        await sut.DisposeAsync();

        dashWrapperMock.Verify(x => x.StopAsync(), Times.Once);
        dashWrapperMock.Verify(x => x.DisposeAsync(), Times.Once);
    }
}
