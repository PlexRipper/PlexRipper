using Reaparr.Environment;

namespace Reaparr.Application.UnitTests;

public class SetupSonarrDownloadClientCommandUnitTests : BaseUnitTest<SetupSonarrDownloadClientCommandHandler>
{
    // ISonarrSettings inherits IBaseSettingsModule<T> which has a static abstract member, making it
    // incompatible with Moq. Inject concrete SonarrSettings instances via TypedParameter instead.

    private SetupSonarrDownloadClientCommandHandler CreateSut(
        SonarrSettings sonarrSettings,
        IntegrationsSettings integrationsSettings,
        NetworkSettingsModule networkSettings
    ) =>
        Mock.Create<SetupSonarrDownloadClientCommandHandler>(
            new TypedParameter(typeof(ISonarrSettings), sonarrSettings),
            new TypedParameter(typeof(IIntegrationsSettings), integrationsSettings),
            new TypedParameter(typeof(INetworkSettings), networkSettings)
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

    private static NetworkSettingsModule ValidNetworkSettings(string? reverseProxyUrl = null, string? basePath = null)
    {
        var settings = NetworkSettingsModule.Create();

        if (reverseProxyUrl is not null)
            settings.ReverseProxyUrl = reverseProxyUrl;

        if (basePath is not null)
            settings.BasePath = basePath;

        return settings;
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenSonarrBaseUrlIsEmpty()
    {
        // Arrange
        var sut = CreateSut(ValidSettings(baseUrl: string.Empty), ValidIntegrationsSettings(), ValidNetworkSettings());

        // Act
        var result = await sut.ExecuteAsync(new SetupSonarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        var commandExecutorMock = Mock.Mock<ICommandExecutor>();
        commandExecutorMock.Verify(
            x => x.Send(It.IsAny<ICommand<Result<List<DownloadClientResourceDTO>>>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        commandExecutorMock.Verify(
            x => x.Send(It.IsAny<ICommand<Result<SonarrDownloadContractDTO>>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenSonarrApiKeyIsEmpty()
    {
        // Arrange
        var sut = CreateSut(ValidSettings(apiKey: string.Empty), ValidIntegrationsSettings(), ValidNetworkSettings());

        // Act
        var result = await sut.ExecuteAsync(new SetupSonarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        var commandExecutorMock = Mock.Mock<ICommandExecutor>();
        commandExecutorMock.Verify(
            x => x.Send(It.IsAny<ICommand<Result<List<DownloadClientResourceDTO>>>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        commandExecutorMock.Verify(
            x => x.Send(It.IsAny<ICommand<Result<SonarrDownloadContractDTO>>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenGetDownloadClientsFails()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<SonarApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Fail("Sonarr unreachable"))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarrApiCreateDownloadClientCommand>).Verifiable(Times.Never);
        Mock.SetupCommand(It.IsAny<SonarApiUpdateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings(), ValidNetworkSettings());

        // Act
        var result = await sut.ExecuteAsync(new SetupSonarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
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

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings(), ValidNetworkSettings());

        // Act
        var result = await sut.ExecuteAsync(new SetupSonarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.DownloadClientId.ShouldBe(21);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
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

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings(), ValidNetworkSettings());

        // Act
        var result = await sut.ExecuteAsync(new SetupSonarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.DownloadClientId.ShouldBe(5);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenCreateDownloadClientFails()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<SonarApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<DownloadClientResourceDTO>()))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarrApiCreateDownloadClientCommand>)
            .ReturnsAsync(Result.Fail("Create failed"))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarApiUpdateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings(), ValidNetworkSettings());

        // Act
        var result = await sut.ExecuteAsync(new SetupSonarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
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

        var sut = CreateSut(
            ValidSettings(),
            ValidIntegrationsSettings(),
            ValidNetworkSettings("https://reaparr.custom-domain.nl", string.Empty)
        );

        // Act
        var result = await sut.ExecuteAsync(new SetupSonarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedResource.ShouldNotBeNull();

        var useSslField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "useSsl");
        useSslField.ShouldNotBeNull();
        useSslField.Value.ShouldBe(true);

        var portField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "port");
        portField.ShouldNotBeNull();
        portField.Value.ShouldBe(443);

        var urlBaseField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "urlBase");
        urlBaseField.ShouldNotBeNull();
        urlBaseField.Value.ShouldBe("api/public/download-client");

        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldBuildResourceWithHttpProxyHostPortAndBasePath_WhenReverseProxyIsHttp()
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
                    return Result.Ok(new SonarrDownloadContractDTO { Id = 2 });
                }
            )
            .Verifiable(Times.Once);

        var sut = CreateSut(
            ValidSettings(),
            ValidIntegrationsSettings(),
            ValidNetworkSettings("http://reaparr.example.com:8080", "/reaparr")
        );

        // Act
        var result = await sut.ExecuteAsync(new SetupSonarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedResource.ShouldNotBeNull();

        var hostField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "host");
        hostField.ShouldNotBeNull();
        hostField.Value.ShouldBe("reaparr.example.com");

        var portField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "port");
        portField.ShouldNotBeNull();
        portField.Value.ShouldBe(8080);

        var useSslField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "useSsl");
        useSslField.ShouldNotBeNull();
        useSslField.Value.ShouldBe(false);

        var urlBaseField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "urlBase");
        urlBaseField.ShouldNotBeNull();
        urlBaseField.Value.ShouldBe("/reaparr/api/public/download-client");

        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldFallbackToLocalhost_WhenReverseProxyUrlIsInvalid()
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
                    return Result.Ok(new SonarrDownloadContractDTO { Id = 3 });
                }
            )
            .Verifiable(Times.Once);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings(), ValidNetworkSettings("not-a-url"));

        // Act
        var result = await sut.ExecuteAsync(new SetupSonarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedResource.ShouldNotBeNull();

        var hostField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "host");
        hostField.ShouldNotBeNull();
        hostField.Value.ShouldBe("localhost");

        var portField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "port");
        portField.ShouldNotBeNull();
        portField.Value.ShouldBe(Mock.Container.Resolve<IAppRuntimeInfo>().AppPort);

        var useSslField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "useSsl");
        useSslField.ShouldNotBeNull();
        useSslField.Value.ShouldBe(false);

        var urlBaseField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "urlBase");
        urlBaseField.ShouldNotBeNull();
        urlBaseField.Value.ShouldBe("api/public/download-client");

        Mock.Mock<ICommandExecutor>().Verify();
    }
}
