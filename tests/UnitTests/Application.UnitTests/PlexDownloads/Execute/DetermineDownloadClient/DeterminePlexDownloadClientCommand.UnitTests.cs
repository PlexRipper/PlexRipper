using Reaparr.Application.Contracts;
using Reaparr.PlexApi.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application.UnitTests;

public class DeterminePlexDownloadClientCommandUnitTests : BaseUnitTest<DeterminePlexDownloadClientCommandHandler>
{
    public DeterminePlexDownloadClientCommandUnitTests()
        : base() { }

    [Test]
    public async Task ShouldReturnDirect_WhenStreamDownloaderIsDisabled()
    {
        // Arrange
        await SetupDatabase(
            91101,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var downloadTask = IDbContext.DownloadTaskMovieFile.First();

        Mock.Mock<IServerSettingsModule>()
            .Setup(x => x.GetAllowStreamDownloader(It.IsAny<string>()))
            .Returns(false)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(
            new DeterminePlexDownloadClientCommand(
                downloadTask.PlexServerId,
                downloadTask.ToKey(),
                $"/library/metadata/{downloadTask.PlexApiRatingKey}"
            ),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(PlexDownloadClientType.Direct);
        Mock.Mock<IServerSettingsModule>().Verify();
    }

    [Test]
    public async Task ShouldReturnDirect_WhenTranscodeUrlSuggestsDirect()
    {
        // Arrange
        await SetupDatabase(
            91102,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var downloadTask = IDbContext.DownloadTaskMovieFile.First();

        Mock.Mock<IServerSettingsModule>()
            .Setup(x => x.GetAllowStreamDownloader(It.IsAny<string>()))
            .Returns(true)
            .Verifiable(Times.Once());

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetTranscodeUrlCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Ok(
                    new GetTranscodeUrlResult
                    {
                        DownloadUrl = string.Empty,
                        SuggestedClientType = PlexDownloadClientType.Direct,
                    }
                )
            )
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(
            new DeterminePlexDownloadClientCommand(
                downloadTask.PlexServerId,
                downloadTask.ToKey(),
                $"/library/metadata/{downloadTask.PlexApiRatingKey}"
            ),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(PlexDownloadClientType.Direct);
        Mock.Mock<IServerSettingsModule>().Verify();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldReturnDash_WhenTranscodeUrlDoesNotSuggestDirect()
    {
        // Arrange
        await SetupDatabase(
            91103,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var downloadTask = IDbContext.DownloadTaskMovieFile.First();

        Mock.Mock<IServerSettingsModule>()
            .Setup(x => x.GetAllowStreamDownloader(It.IsAny<string>()))
            .Returns(true)
            .Verifiable(Times.Once());

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetTranscodeUrlCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Ok(
                    new GetTranscodeUrlResult
                    {
                        DownloadUrl = "https://plex.example/video/:/transcode/universal/start.mpd",
                        SuggestedClientType = PlexDownloadClientType.Dash,
                    }
                )
            )
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(
            new DeterminePlexDownloadClientCommand(
                downloadTask.PlexServerId,
                downloadTask.ToKey(),
                $"/library/metadata/{downloadTask.PlexApiRatingKey}"
            ),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(PlexDownloadClientType.Dash);
        Mock.Mock<IServerSettingsModule>().Verify();
        Mock.Mock<ICommandExecutor>().Verify();
    }
}
