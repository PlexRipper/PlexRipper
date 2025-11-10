using Autofac;
using Reaparr.PlexApi;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application.UnitTests;

public class PlexDownloadClientSetupUnitTests : BaseUnitTest
{
    public PlexDownloadClientSetupUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenNullDownloadTaskIsGiven()
    {
        //Arrange
        await SetupDatabase(82345);
        var sut = Mock.Create<PlexDownloadClient>(
            new NamedParameter(
                "downloadWorkerFactory",
                (DownloadWorkerTask task) => Mock.Create<DownloadWorker>(new NamedParameter("downloadWorkerTask", task))
            ),
            new NamedParameter(
                "clientFactory",
                (PlexApiClientOptions options) => Mock.Create<PlexApiClient>(new NamedParameter("options", options))
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
