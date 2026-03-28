using Autofac;
using FastEndpoints;
using Reaparr.Environment;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application.UnitTests;

public class SetupRadarrDownloadClientCommandUnitTests : BaseUnitTest<SetupRadarrDownloadClientCommandHandler>
{
    public SetupRadarrDownloadClientCommandUnitTests()
        : base() { }

    // IRadarrSettings inherits IBaseSettingsModule<T> which has a static abstract member, making it
    // incompatible with Moq. Inject concrete RadarrSettings instances via TypedParameter instead.

    private SetupRadarrDownloadClientCommandHandler CreateSut(
        RadarrSettings radarrSettings,
        IntegrationsSettings integrationsSettings,
        NetworkSettingsModule networkSettings
    ) =>
        Mock.Create<SetupRadarrDownloadClientCommandHandler>(
            new TypedParameter(typeof(IRadarrSettings), radarrSettings),
            new TypedParameter(typeof(IIntegrationsSettings), integrationsSettings),
            new TypedParameter(typeof(INetworkSettings), networkSettings)
        );

    private static RadarrSettings ValidSettings(
        string baseUrl = "http://localhost:7878",
        string apiKey = "some-api-key"
    ) =>
        new()
        {
            IsConfigured = false,
            RadarrBaseUrl = baseUrl,
            RadarrApiKey = apiKey,
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
    public async Task ShouldReturnFailedResult_WhenRadarrBaseUrlIsEmpty()
    {
        // Arrange
        var sut = CreateSut(ValidSettings(baseUrl: string.Empty), ValidIntegrationsSettings(), ValidNetworkSettings());

        // Act
        var result = await sut.ExecuteAsync(new SetupRadarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        var commandExecutorMock = Mock.Mock<ICommandExecutor>();
        commandExecutorMock.Verify(
            x =>
                x.Send(
                    It.IsAny<ICommand<Result<List<RadarrDownloadClientResourceDTO>>>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        commandExecutorMock.Verify(
            x => x.Send(It.IsAny<ICommand<Result<RadarrDownloadClientResourceDTO>>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenRadarrApiKeyIsEmpty()
    {
        // Arrange
        var sut = CreateSut(ValidSettings(apiKey: string.Empty), ValidIntegrationsSettings(), ValidNetworkSettings());

        // Act
        var result = await sut.ExecuteAsync(new SetupRadarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        var commandExecutorMock = Mock.Mock<ICommandExecutor>();
        commandExecutorMock.Verify(
            x =>
                x.Send(
                    It.IsAny<ICommand<Result<List<RadarrDownloadClientResourceDTO>>>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        commandExecutorMock.Verify(
            x => x.Send(It.IsAny<ICommand<Result<RadarrDownloadClientResourceDTO>>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenGetDownloadClientsFails()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<RadarrApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Fail("Radarr unreachable"))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiCreateDownloadClientCommand>).Verifiable(Times.Never);
        Mock.SetupCommand(It.IsAny<RadarrApiUpdateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings(), ValidNetworkSettings());

        // Act
        var result = await sut.ExecuteAsync(new SetupRadarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldCreateDownloadClient_WhenNoneExists()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<RadarrApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<RadarrDownloadClientResourceDTO>()))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiCreateDownloadClientCommand>)
            .ReturnsAsync(Result.Ok(new RadarrDownloadClientResourceDTO { Id = 42 }))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiUpdateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings(), ValidNetworkSettings());

        // Act
        var result = await sut.ExecuteAsync(new SetupRadarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.DownloadClientId.ShouldBe(42);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldUpdateExistingDownloadClient_WhenOneAlreadyExists()
    {
        // Arrange
        var existingClient = new RadarrDownloadClientResourceDTO { Id = 7, Name = "Reaparr DownloadClient" };

        Mock.SetupCommand(It.IsAny<RadarrApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<RadarrDownloadClientResourceDTO> { existingClient }))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiUpdateDownloadClientCommand>)
            .ReturnsAsync(Result.Ok(new RadarrDownloadClientResourceDTO { Id = 7 }))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiCreateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings(), ValidNetworkSettings());

        // Act
        var result = await sut.ExecuteAsync(new SetupRadarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.DownloadClientId.ShouldBe(7);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenCreateDownloadClientFails()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<RadarrApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<RadarrDownloadClientResourceDTO>()))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiCreateDownloadClientCommand>)
            .ReturnsAsync(Result.Fail("Create failed"))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiUpdateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings(), ValidNetworkSettings());

        // Act
        var result = await sut.ExecuteAsync(new SetupRadarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldBuildResourceWithSslEnabledAndPort443_WhenReaparrUriIsHttpsCustomDomain()
    {
        // Arrange
        RadarrDownloadContractDTO? capturedResource = null;

        Mock.SetupCommand(It.IsAny<RadarrApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<RadarrDownloadClientResourceDTO>()))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiCreateDownloadClientCommand>)
            .ReturnsAsync(
                (RadarrApiCreateDownloadClientCommand cmd, CancellationToken _) =>
                {
                    capturedResource = cmd.Resource;
                    return Result.Ok(new RadarrDownloadClientResourceDTO { Id = 1 });
                }
            )
            .Verifiable(Times.Once);

        var sut = CreateSut(
            ValidSettings(),
            ValidIntegrationsSettings(),
            ValidNetworkSettings("https://reaparr.custom-domain.nl", string.Empty)
        );

        // Act
        var result = await sut.ExecuteAsync(new SetupRadarrDownloadClientCommand(), CancellationToken);

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
        RadarrDownloadContractDTO? capturedResource = null;

        Mock.SetupCommand(It.IsAny<RadarrApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<RadarrDownloadClientResourceDTO>()))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiCreateDownloadClientCommand>)
            .ReturnsAsync(
                (RadarrApiCreateDownloadClientCommand cmd, CancellationToken _) =>
                {
                    capturedResource = cmd.Resource;
                    return Result.Ok(new RadarrDownloadClientResourceDTO { Id = 2 });
                }
            )
            .Verifiable(Times.Once);

        var sut = CreateSut(
            ValidSettings(),
            ValidIntegrationsSettings(),
            ValidNetworkSettings("http://reaparr.example.com:8080", "/reaparr")
        );

        // Act
        var result = await sut.ExecuteAsync(new SetupRadarrDownloadClientCommand(), CancellationToken);

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
        RadarrDownloadContractDTO? capturedResource = null;

        Mock.SetupCommand(It.IsAny<RadarrApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<RadarrDownloadClientResourceDTO>()))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiCreateDownloadClientCommand>)
            .ReturnsAsync(
                (RadarrApiCreateDownloadClientCommand cmd, CancellationToken _) =>
                {
                    capturedResource = cmd.Resource;
                    return Result.Ok(new RadarrDownloadClientResourceDTO { Id = 3 });
                }
            )
            .Verifiable(Times.Once);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings(), ValidNetworkSettings("not-a-url"));

        // Act
        var result = await sut.ExecuteAsync(new SetupRadarrDownloadClientCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedResource.ShouldNotBeNull();

        var hostField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "host");
        hostField.ShouldNotBeNull();
        hostField.Value.ShouldBe("localhost");

        var portField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "port");
        portField.ShouldNotBeNull();
        portField.Value.ShouldBe(EnvironmentExtensions.GetPort);

        var useSslField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "useSsl");
        useSslField.ShouldNotBeNull();
        useSslField.Value.ShouldBe(false);

        var urlBaseField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "urlBase");
        urlBaseField.ShouldNotBeNull();
        urlBaseField.Value.ShouldBe("api/public/download-client");

        Mock.Mock<ICommandExecutor>().Verify();
    }
}
