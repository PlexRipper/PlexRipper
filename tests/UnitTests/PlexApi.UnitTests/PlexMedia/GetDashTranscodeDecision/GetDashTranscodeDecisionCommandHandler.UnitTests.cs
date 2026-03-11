using System.Net;
using LukeHagar.PlexAPI.SDK;
using LukeHagar.PlexAPI.SDK.Models.Components;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using Microsoft.EntityFrameworkCore;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi.UnitTests;

public class GetDashTranscodeDecisionCommandHandlerUnitTests : BaseUnitTest<GetDashTranscodeDecisionCommandHandler>
{
    public GetDashTranscodeDecisionCommandHandlerUnitTests(ITestOutputHelper output)
        : base(output) { }

    private static GetDashTranscodeDecisionCommand CreateCommand(int plexServerId) =>
        new(
            plexServerId,
            new MakeDecisionRequest
            {
                Path = "/library/metadata/56828",
                ClientIdentifier = "1x6jbxuls57ip8sg6pr5sxsn",
                TranscodeSessionId = "vyoe41m5hrmlotwc6zyocadz",
                XPlexSessionIdentifier = "yzjqymlmh5ssjfm51hr881pe",
            }
        );

    [Fact]
    public async Task ShouldReturnFailedResult_WhenPlexServerHasNoToken()
    {
        // Arrange
        await SetupDatabase(4101, config => config.PlexServerCount = 1);
        var dbContext = IDbContext;

        var plexServer = await dbContext.PlexServers.FirstOrDefaultAsync(CancellationToken);
        plexServer.ShouldNotBeNull();

        var command = CreateCommand(plexServer.Id);

        Mock.Mock<IPlexApiClientFactory>()
            .Setup(x => x.CreateClient(It.IsAny<string>(), It.IsAny<PlexApiClientOptions>()))
            .Returns(new Mock<IPlexAPI>().Object)
            .Verifiable(Times.Never);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains("authenticationToken"));

        Mock.Mock<IPlexApiClientFactory>()
            .Verify(x => x.CreateClient(It.IsAny<string>(), It.IsAny<PlexApiClientOptions>()), Times.Never());
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenDecisionResponseIsMissingMediaContainer()
    {
        // Arrange
        await SetupDatabase(
            4102,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
            }
        );
        var dbContext = IDbContext;

        var plexServer = await dbContext.PlexServers.FirstOrDefaultAsync(CancellationToken);
        plexServer.ShouldNotBeNull();

        var decisionResponse = FakePlexApiData.GetMakeDecisionResponse(
            HttpStatusCode.OK,
            new Seed(4102),
            new MediaContainerWithDecision { MediaContainer = null }
        );

        var plexApiMock = new Mock<IPlexAPI>();
        plexApiMock
            .Setup(x => x.Transcoder.MakeDecisionAsync(It.IsAny<MakeDecisionRequest>()))
            .ReturnsAsync(decisionResponse)
            .Verifiable(Times.Once);

        Mock.Mock<IPlexApiClientFactory>()
            .Setup(x => x.CreateClient(It.IsAny<string>(), It.IsAny<PlexApiClientOptions>()))
            .Returns(plexApiMock.Object)
            .Verifiable(Times.Once);

        var command = CreateCommand(plexServer.Id);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains("missing MediaContainer"));

        Mock.Mock<IPlexApiClientFactory>().Verify();
        plexApiMock.Verify();
    }

    [Fact]
    public async Task ShouldReturnMappedDecisionSummary_WhenDecisionContainsVideoAndAudioStreams()
    {
        // Arrange
        await SetupDatabase(
            4103,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
            }
        );
        var dbContext = IDbContext;

        var plexServer = await dbContext.PlexServers.FirstOrDefaultAsync(CancellationToken);
        plexServer.ShouldNotBeNull();

        var decisionResponse = FakePlexApiData.GetMakeDecisionResponse(HttpStatusCode.OK, new Seed(4103));

        var plexApiMock = new Mock<IPlexAPI>();
        plexApiMock
            .Setup(x => x.Transcoder.MakeDecisionAsync(It.IsAny<MakeDecisionRequest>()))
            .ReturnsAsync(decisionResponse)
            .Verifiable(Times.Once);

        Mock.Mock<IPlexApiClientFactory>()
            .Setup(x => x.CreateClient(It.IsAny<string>(), It.IsAny<PlexApiClientOptions>()))
            .Returns(plexApiMock.Object)
            .Verifiable(Times.Once);

        var command = CreateCommand(plexServer.Id);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.GeneralDecisionCode.ShouldBe("1001");
        result.Value.GeneralDecisionText.ShouldBe("Direct play not available; Conversion OK.");
        result.Value.TranscodeDecisionCode.ShouldBe("1001");
        result.Value.TranscodeDecisionText.ShouldBe("Direct play not available; Conversion OK.");
        result.Value.VideoDecision.ShouldBe("Copy");
        result.Value.AudioDecision.ShouldBe("Copy");
        result.Value.TranscodedQuality.ShouldBe(VideoQuality.FullHD);

        Mock.Mock<IPlexApiClientFactory>().Verify();
        plexApiMock.Verify();
    }

    [Fact]
    public async Task ShouldResolveQualityFromWidth_WhenStreamTitleDoesNotContainAResolution()
    {
        // Arrange
        await SetupDatabase(
            4104,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
            }
        );
        var dbContext = IDbContext;

        var plexServer = await dbContext.PlexServers.FirstOrDefaultAsync(CancellationToken);
        plexServer.ShouldNotBeNull();

        var mediaContainer = FakePlexApiData.GetMakeDecisionMediaContainerWithWidthFallback();
        var decisionResponse = FakePlexApiData.GetMakeDecisionResponse(
            HttpStatusCode.OK,
            new Seed(4104),
            mediaContainer
        );

        var plexApiMock = new Mock<IPlexAPI>();
        plexApiMock
            .Setup(x => x.Transcoder.MakeDecisionAsync(It.IsAny<MakeDecisionRequest>()))
            .ReturnsAsync(decisionResponse)
            .Verifiable(Times.Once);

        Mock.Mock<IPlexApiClientFactory>()
            .Setup(x => x.CreateClient(It.IsAny<string>(), It.IsAny<PlexApiClientOptions>()))
            .Returns(plexApiMock.Object)
            .Verifiable(Times.Once);

        var command = CreateCommand(plexServer.Id);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TranscodedQuality.ShouldBe(VideoQuality.UHD_4K);

        Mock.Mock<IPlexApiClientFactory>().Verify();
        plexApiMock.Verify();
    }

    public static TheoryData<Action<MakeDecisionMediaContainerConfig>, VideoQuality> QualityCases =>
        new()
        {
            {
                config =>
                {
                    config.MediaVideoResolution = "720p";
                    config.VideoStreams.Clear();
                    config.VideoStreams.Add(new MakeDecisionVideoStreamConfig { DisplayTitle = "Video Stream" });
                },
                VideoQuality.HD
            },
            {
                config =>
                {
                    config.MediaVideoResolution = "weird-resolution";
                    config.VideoStreams.Clear();
                    config.VideoStreams.Add(
                        new MakeDecisionVideoStreamConfig
                        {
                            DisplayTitle = "2160p",
                            ExtendedDisplayTitle = "4k",
                            Width = 3840,
                            Height = 2160,
                        }
                    );
                },
                VideoQuality.UHD_4K
            },
            {
                config =>
                {
                    config.MediaVideoResolution = null;
                    config.MediaWidth = null;
                    config.MediaHeight = null;
                    config.VideoStreams.Clear();
                    config.VideoStreams.Add(
                        new MakeDecisionVideoStreamConfig
                        {
                            DisplayTitle = "Video Stream",
                            ExtendedDisplayTitle = "1440p",
                            Width = null,
                            Height = null,
                        }
                    );
                },
                VideoQuality.QHD
            },
            {
                config =>
                {
                    config.MediaVideoResolution = null;
                    config.MediaWidth = null;
                    config.MediaHeight = null;
                    config.VideoStreams.Clear();
                    config.VideoStreams.Add(
                        new MakeDecisionVideoStreamConfig
                        {
                            DisplayTitle = "Video Stream",
                            ExtendedDisplayTitle = "Video Stream",
                            Width = 1280,
                            Height = null,
                        }
                    );
                },
                VideoQuality.HD
            },
            {
                config =>
                {
                    config.MediaVideoResolution = null;
                    config.MediaWidth = null;
                    config.MediaHeight = null;
                    config.VideoStreams.Clear();
                    config.VideoStreams.Add(
                        new MakeDecisionVideoStreamConfig
                        {
                            DisplayTitle = "Video Stream",
                            ExtendedDisplayTitle = "Video Stream",
                            Width = null,
                            Height = 576,
                        }
                    );
                },
                VideoQuality.DVD
            },
            {
                config =>
                {
                    config.MediaVideoResolution = null;
                    config.MediaWidth = 640;
                    config.MediaHeight = null;
                    config.VideoStreams.Clear();
                    config.VideoStreams.Add(
                        new MakeDecisionVideoStreamConfig
                        {
                            DisplayTitle = "Video Stream",
                            ExtendedDisplayTitle = "Video Stream",
                            Width = null,
                            Height = null,
                        }
                    );
                },
                VideoQuality.nHD
            },
        };

    [Theory]
    [MemberData(nameof(QualityCases))]
    public async Task ShouldResolveExpectedQuality_ForDifferentDecisionPayloadShapes(
        Action<MakeDecisionMediaContainerConfig> options,
        VideoQuality expectedQuality
    )
    {
        // Arrange
        await SetupDatabase(
            4105,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
            }
        );
        var dbContext = IDbContext;

        var plexServer = await dbContext.PlexServers.FirstOrDefaultAsync(CancellationToken);
        plexServer.ShouldNotBeNull();

        var mediaContainer = FakePlexApiData.GetMakeDecisionMediaContainer(options);
        var decisionResponse = FakePlexApiData.GetMakeDecisionResponse(
            HttpStatusCode.OK,
            new Seed(4105),
            mediaContainer
        );

        var plexApiMock = new Mock<IPlexAPI>();
        plexApiMock
            .Setup(x => x.Transcoder.MakeDecisionAsync(It.IsAny<MakeDecisionRequest>()))
            .ReturnsAsync(decisionResponse)
            .Verifiable(Times.Once);

        Mock.Mock<IPlexApiClientFactory>()
            .Setup(x => x.CreateClient(It.IsAny<string>(), It.IsAny<PlexApiClientOptions>()))
            .Returns(plexApiMock.Object)
            .Verifiable(Times.Once);

        var command = CreateCommand(plexServer.Id);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TranscodedQuality.ShouldBe(expectedQuality);

        Mock.Mock<IPlexApiClientFactory>().Verify();
        plexApiMock.Verify();
    }

    [Fact]
    public async Task ShouldChooseHighestAvailableQuality_WhenDecisionContainsMultipleVideoVariants()
    {
        // Arrange
        await SetupDatabase(
            4106,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
            }
        );
        var dbContext = IDbContext;

        var plexServer = await dbContext.PlexServers.FirstOrDefaultAsync(CancellationToken);
        plexServer.ShouldNotBeNull();

        var mediaContainer = FakePlexApiData.GetMakeDecisionMediaContainer(config =>
        {
            config.MediaVideoResolution = "480p";
            config.VideoStreams.Clear();
            config.VideoStreams.Add(
                new MakeDecisionVideoStreamConfig
                {
                    DisplayTitle = "480p",
                    ExtendedDisplayTitle = "480p",
                    Width = 854,
                    Height = 480,
                }
            );
            config.VideoStreams.Add(
                new MakeDecisionVideoStreamConfig
                {
                    DisplayTitle = "1080p",
                    ExtendedDisplayTitle = "1080p",
                    Width = 1920,
                    Height = 1080,
                }
            );
            config.VideoStreams.Add(
                new MakeDecisionVideoStreamConfig
                {
                    DisplayTitle = "2160p",
                    ExtendedDisplayTitle = "2160p",
                    Width = 3840,
                    Height = 2160,
                }
            );
        });
        var decisionResponse = FakePlexApiData.GetMakeDecisionResponse(
            HttpStatusCode.OK,
            new Seed(4106),
            mediaContainer
        );

        var plexApiMock = new Mock<IPlexAPI>();
        plexApiMock
            .Setup(x => x.Transcoder.MakeDecisionAsync(It.IsAny<MakeDecisionRequest>()))
            .ReturnsAsync(decisionResponse)
            .Verifiable(Times.Once);

        Mock.Mock<IPlexApiClientFactory>()
            .Setup(x => x.CreateClient(It.IsAny<string>(), It.IsAny<PlexApiClientOptions>()))
            .Returns(plexApiMock.Object)
            .Verifiable(Times.Once);

        var command = CreateCommand(plexServer.Id);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TranscodedQuality.ShouldBe(VideoQuality.UHD_4K);

        Mock.Mock<IPlexApiClientFactory>().Verify();
        plexApiMock.Verify();
    }
}
