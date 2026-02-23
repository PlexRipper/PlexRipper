using Autofac;
using Reaparr.PublicAPI.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application.UnitTests;

public class SetupSonarrDownloadClientCommandUnitTests : BaseUnitTest<SetupSonarrDownloadClientCommandHandler>
{
    public SetupSonarrDownloadClientCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    // ISonarrSettings inherits IBaseSettingsModule<T> which has a static abstract member, making it
    // incompatible with Moq. Inject concrete SonarrSettings instances via TypedParameter instead.

    private SetupSonarrDownloadClientCommandHandler CreateSut(
        SonarrSettings sonarrSettings,
        IntegrationsSettings integrationsSettings
    ) =>
        Mock.Create<SetupSonarrDownloadClientCommandHandler>(
            new TypedParameter(typeof(ISonarrSettings), sonarrSettings),
            new TypedParameter(typeof(IIntegrationsSettings), integrationsSettings)
        );

    private static SonarrSettings ValidSettings(
        string baseUrl = "http://localhost:8989",
        string apiKey = "some-api-key"
    ) =>
        new()
        {
            IsConfigured = false,
            SonarrBaseUrl = baseUrl,
            SonarrApiKey = apiKey,
        };

    private static IntegrationsSettings ValidIntegrationsSettings() => IntegrationsSettings.Create();

    [Fact]
    public async Task ShouldReturnFailedResult_WhenSonarrBaseUrlIsEmpty()
    {
        // Arrange
        var sut = CreateSut(ValidSettings(baseUrl: string.Empty), ValidIntegrationsSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupSonarrDownloadClientCommand(new Uri("http://localhost:7575")),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenSonarrApiKeyIsEmpty()
    {
        // Arrange
        var sut = CreateSut(ValidSettings(apiKey: string.Empty), ValidIntegrationsSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupSonarrDownloadClientCommand(new Uri("http://localhost:7575")),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenGetDownloadClientsFails()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<SonarApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Fail("Sonarr unreachable"))
            .Verifiable(Times.Once);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupSonarrDownloadClientCommand(new Uri("http://localhost:7575")),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Fact]
    public async Task ShouldCreateDownloadClient_WhenNoneExists()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<SonarApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<DownloadClientResourceDTO>()))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarrApiCreateDownloadClientCommand>)
            .ReturnsAsync(Result.Ok(new SonarrDownloadContractDTO { Id = 21 }))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarApiUpdateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupSonarrDownloadClientCommand(new Uri("http://localhost:7575")),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.DownloadClientId.ShouldBe(21);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Fact]
    public async Task ShouldUpdateExistingDownloadClient_WhenOneAlreadyExists()
    {
        // Arrange
        var existingClient = new DownloadClientResourceDTO { Id = 5, Name = "Reaparr DownloadClient" };

        Mock.SetupCommand(It.IsAny<SonarApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<DownloadClientResourceDTO> { existingClient }))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarApiUpdateDownloadClientCommand>)
            .ReturnsAsync(Result.Ok(new SonarrDownloadContractDTO { Id = 5 }))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarrApiCreateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupSonarrDownloadClientCommand(new Uri("http://localhost:7575")),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.DownloadClientId.ShouldBe(5);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenCreateDownloadClientFails()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<SonarApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<DownloadClientResourceDTO>()))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarrApiCreateDownloadClientCommand>)
            .ReturnsAsync(Result.Fail("Create failed"))
            .Verifiable(Times.Once);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupSonarrDownloadClientCommand(new Uri("http://localhost:7575")),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Fact]
    public async Task ShouldBuildResourceWithSslEnabledAndPort443_WhenReaparrUriIsHttpsCustomDomain()
    {
        // Arrange
        SonarrDownloadContractDTO? capturedResource = null;

        Mock.SetupCommand(It.IsAny<SonarApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<DownloadClientResourceDTO>()))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarrApiCreateDownloadClientCommand>)
            .ReturnsAsync(
                (SonarrApiCreateDownloadClientCommand cmd, CancellationToken _) =>
                {
                    capturedResource = cmd.Resource;
                    return Result.Ok(new SonarrDownloadContractDTO { Id = 1 });
                }
            )
            .Verifiable(Times.Once);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupSonarrDownloadClientCommand(new Uri("https://radarr.custom-domain.nl")),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedResource.ShouldNotBeNull();

        var useSslField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "useSsl");
        useSslField.ShouldNotBeNull();
        useSslField.Value.ShouldBe(true);

        var portField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "port");
        portField.ShouldNotBeNull();
        portField.Value.ShouldBe(443);

        Mock.Mock<ICommandExecutor>().Verify();
    }
}
