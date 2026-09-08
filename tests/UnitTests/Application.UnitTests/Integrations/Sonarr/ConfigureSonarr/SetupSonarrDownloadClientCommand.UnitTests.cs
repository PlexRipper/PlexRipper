namespace Reaparr.Application.UnitTests;

public class SetupSonarrDownloadClientCommandUnitTests : BaseUnitTest<SetupSonarrDownloadClientCommandHandler>
{
    private async Task<SonarrIntegration> SetupIntegrationAsync()
    {
        await SetupDatabase(62600, config => config.SonarrIntegrationCount = 1);
        return await IDbContext.SonarrIntegrations.SingleAsync(CancellationToken);
    }

    private SetupSonarrDownloadClientCommandHandler CreateSut(NetworkSettingsModule networkSettings) =>
        Mock.Create<SetupSonarrDownloadClientCommandHandler>(
            new TypedParameter(typeof(INetworkSettings), networkSettings)
        );

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
        var integration = await SetupIntegrationAsync();
        await IDbContext
            .SonarrIntegrations.Where(x => x.Id == integration.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.BaseUrl, string.Empty), CancellationToken);
        var sut = CreateSut(ValidNetworkSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupSonarrDownloadClientCommand { IntegrationId = integration.Id },
            CancellationToken
        );

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
    public async Task ShouldReturnFailedResult_WhenIntegrationIsMissing()
    {
        // Arrange
        var sut = CreateSut(ValidNetworkSettings());

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
        var integration = await SetupIntegrationAsync();
        Mock.SetupCommand(It.IsAny<SonarApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Fail("Sonarr unreachable"))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarrApiCreateDownloadClientCommand>).Verifiable(Times.Never);
        Mock.SetupCommand(It.IsAny<SonarApiUpdateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidNetworkSettings());
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendIntegrationSetupProgressAsync(It.IsAny<IntegrationSetupProgressDTO>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await sut.ExecuteAsync(
            new SetupSonarrDownloadClientCommand { IntegrationId = integration.Id },
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldCreateDownloadClient_WhenNoneExists()
    {
        // Arrange
        var integration = await SetupIntegrationAsync();
        Mock.SetupCommand(It.IsAny<SonarApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<DownloadClientResourceDTO>()))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarrApiCreateDownloadClientCommand>)
            .ReturnsAsync(Result.Ok(new SonarrDownloadContractDTO { Id = 21 }))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarApiUpdateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidNetworkSettings());
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendIntegrationSetupProgressAsync(It.IsAny<IntegrationSetupProgressDTO>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await sut.ExecuteAsync(
            new SetupSonarrDownloadClientCommand { IntegrationId = integration.Id },
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.DownloadClientId.ShouldBe(21);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldUpdateExistingDownloadClient_WhenOneAlreadyExists()
    {
        // Arrange
        var integration = await SetupIntegrationAsync();
        var existingClient = new DownloadClientResourceDTO { Id = 5, Name = "Reaparr DownloadClient" };

        Mock.SetupCommand(It.IsAny<SonarApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<DownloadClientResourceDTO> { existingClient }))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarApiUpdateDownloadClientCommand>)
            .ReturnsAsync(Result.Ok(new SonarrDownloadContractDTO { Id = 5 }))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarrApiCreateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidNetworkSettings());
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendIntegrationSetupProgressAsync(It.IsAny<IntegrationSetupProgressDTO>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await sut.ExecuteAsync(
            new SetupSonarrDownloadClientCommand { IntegrationId = integration.Id },
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.DownloadClientId.ShouldBe(5);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenCreateDownloadClientFails()
    {
        // Arrange
        var integration = await SetupIntegrationAsync();
        Mock.SetupCommand(It.IsAny<SonarApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<DownloadClientResourceDTO>()))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarrApiCreateDownloadClientCommand>)
            .ReturnsAsync(Result.Fail("Create failed"))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<SonarApiUpdateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidNetworkSettings());
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendIntegrationSetupProgressAsync(It.IsAny<IntegrationSetupProgressDTO>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await sut.ExecuteAsync(
            new SetupSonarrDownloadClientCommand { IntegrationId = integration.Id },
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldBuildResourceWithSslEnabledAndPort443_WhenReaparrUriIsHttpsCustomDomain()
    {
        // Arrange
        var integration = await SetupIntegrationAsync();
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

        var sut = CreateSut(ValidNetworkSettings("https://reaparr.custom-domain.nl", string.Empty));
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendIntegrationSetupProgressAsync(It.IsAny<IntegrationSetupProgressDTO>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await sut.ExecuteAsync(
            new SetupSonarrDownloadClientCommand { IntegrationId = integration.Id },
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

        var urlBaseField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "urlBase");
        urlBaseField.ShouldNotBeNull();
        urlBaseField.Value.ShouldBe($"api/public/integrations/{integration.Id}/download-client");

        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldBuildResourceWithHttpProxyHostPortAndBasePath_WhenReverseProxyIsHttp()
    {
        // Arrange
        var integration = await SetupIntegrationAsync();
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

        var sut = CreateSut(ValidNetworkSettings("http://reaparr.example.com:8080", "/reaparr"));
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendIntegrationSetupProgressAsync(It.IsAny<IntegrationSetupProgressDTO>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await sut.ExecuteAsync(
            new SetupSonarrDownloadClientCommand { IntegrationId = integration.Id },
            CancellationToken
        );

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
        urlBaseField.Value.ShouldBe($"/reaparr/api/public/integrations/{integration.Id}/download-client");

        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldFallbackToLocalhost_WhenReverseProxyUrlIsInvalid()
    {
        // Arrange
        var integration = await SetupIntegrationAsync();
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

        var sut = CreateSut(ValidNetworkSettings("not-a-url"));
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendIntegrationSetupProgressAsync(It.IsAny<IntegrationSetupProgressDTO>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await sut.ExecuteAsync(
            new SetupSonarrDownloadClientCommand { IntegrationId = integration.Id },
            CancellationToken
        );

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
        urlBaseField.Value.ShouldBe($"api/public/integrations/{integration.Id}/download-client");

        Mock.Mock<ICommandExecutor>().Verify();
    }
}
