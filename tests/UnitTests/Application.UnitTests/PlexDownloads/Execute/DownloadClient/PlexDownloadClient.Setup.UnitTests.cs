using Autofac;
using PlexApi.Contracts;
using PlexRipper.PlexApi;

namespace PlexRipper.Application.UnitTests;

public class PlexDownloadClientSetupUnitTests : BaseUnitTest
{
    public PlexDownloadClientSetupUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenNullDownloadTaskIsGiven()
    {
        //Arrange
        await SetupDatabase(82345);
        var sut = mock.Create<PlexDownloadClient>(
            new NamedParameter(
                "downloadWorkerFactory",
                (DownloadWorkerTask task) => mock.Create<DownloadWorker>(new NamedParameter("downloadWorkerTask", task))
            ),
            new NamedParameter(
                "clientFactory",
                (PlexApiClientOptions options) => mock.Create<PlexApiClient>(new NamedParameter("options", options))
            )
        );

        // Act
        var result = await sut.Setup(
            new DownloadTaskKey
            {
                Type = DownloadTaskType.None,
                Id = default,
                PlexServerId = 0,
                PlexLibraryId = 0,
            },
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }
}
