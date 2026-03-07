using System.IO;
using System.Reactive.Linq;
using Autofac;
using Downloader;
using FastEndpoints;
using Reaparr.Data.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application.UnitTests;

public class DirectPlexDownloadClientDisposeAsyncUnitTests : BaseUnitTest<DirectPlexDownloadClient>
{
    public DirectPlexDownloadClientDisposeAsyncUnitTests(ITestOutputHelper output)
        : base(output) { }

    // -------------------------------------------------------------------------
    // Shared helpers
    // -------------------------------------------------------------------------

    private DirectPlexDownloadClient CreateSut()
    {
        // IDownloadManagerSettings.DownloadSegments is read in the SUT constructor
        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.DownloadSegments).Returns(1);

        // CreateDownloadFileStreamCommand returns Result<Stream>
        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result<Stream>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(Stream.Null));

        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        return Mock.Create<DirectPlexDownloadClient>();
    }

    // -------------------------------------------------------------------------
    // Tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ShouldDisposeWithoutError_WhenDisposeAsyncIsCalledWithoutPriorStart()
    {
        // Arrange — create the SUT without calling Start.
        var sut = CreateSut();

        // Act
        var act = async () => await sut.DisposeAsync();

        // Assert — no throw even though no download was started
        await act.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task ShouldBeIdempotent_WhenDisposeAsyncIsCalledMultipleTimes()
    {
        // Arrange
        var sut = CreateSut();

        // Act — dispose twice; second call must be a no-op, not throw
        var act = async () =>
        {
            await sut.DisposeAsync();
            await sut.DisposeAsync();
        };

        // Assert
        await act.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task ShouldDisposeDbContext_WhenDisposeAsyncIsCalled()
    {
        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.DownloadSegments).Returns(1);

        var dbContextMock = new Mock<IReaparrDbContext>();
        dbContextMock.Setup(x => x.Dispose()).Verifiable(Times.Once);

        var dbContextFactoryMock = new Mock<IReaparrDbContextFactory>();
        dbContextFactoryMock.Setup(x => x.Create()).Returns(dbContextMock.Object);

        var sut = Mock.Create<DirectPlexDownloadClient>(
            new NamedParameter("dbContextFactory", dbContextFactoryMock.Object)
        );

        await sut.DisposeAsync();

        dbContextMock.Verify();
    }
}
