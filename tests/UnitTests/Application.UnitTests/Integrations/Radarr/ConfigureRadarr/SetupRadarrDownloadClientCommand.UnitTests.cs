namespace Reaparr.Application.UnitTests;

public class SetupRadarrDownloadClientCommandUnitTests : BaseUnitTest<SetupRadarrDownloadClientCommandHandler>
{
    private async Task<RadarrIntegration> SetupIntegrationAsync()
    {
        await SetupDatabase(55300, config => config.RadarrIntegrationCount = 1);
        return await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken);
    }

    private SetupRadarrDownloadClientCommandHandler CreateSut(NetworkSettingsModule networkSettings) =>
        Mock.Create<SetupRadarrDownloadClientCommandHandler>(
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
    public async Task ShouldReturnFailedResult_WhenRadarrBaseUrlIsEmpty()
    {
        // Arrange
        var integration = await SetupIntegrationAsync();
        await IDbContext
            .RadarrIntegrations.Where(x => x.Id == integration.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.BaseUrl, string.Empty), CancellationToken);
        var sut = CreateSut(ValidNetworkSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupRadarrDownloadClientCommand { IntegrationId = integration.Id },
            CancellationToken
        );

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
    public async Task ShouldReturnFailedResult_WhenIntegrationIsMissing()
    {
        // Arrange
        var sut = CreateSut(ValidNetworkSettings());

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
        var integration = await SetupIntegrationAsync();
        Mock.SetupCommand(It.IsAny<RadarrApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Fail("Radarr unreachable"))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiCreateDownloadClientCommand>).Verifiable(Times.Never);
        Mock.SetupCommand(It.IsAny<RadarrApiUpdateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidNetworkSettings());
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendIntegrationSetupProgressAsync(It.IsAny<IntegrationSetupProgressDTO>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await sut.ExecuteAsync(
            new SetupRadarrDownloadClientCommand { IntegrationId = integration.Id },
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
        Mock.SetupCommand(It.IsAny<RadarrApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<RadarrDownloadClientResourceDTO>()))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiCreateDownloadClientCommand>)
            .ReturnsAsync(Result.Ok(new RadarrDownloadClientResourceDTO { Id = 42 }))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiUpdateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidNetworkSettings());
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendIntegrationSetupProgressAsync(It.IsAny<IntegrationSetupProgressDTO>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await sut.ExecuteAsync(
            new SetupRadarrDownloadClientCommand { IntegrationId = integration.Id },
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.DownloadClientId.ShouldBe(42);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldUpdateExistingDownloadClient_WhenOneAlreadyExists()
    {
        // Arrange
        var integration = await SetupIntegrationAsync();
        var existingClient = new RadarrDownloadClientResourceDTO { Id = 7, Name = "Reaparr DownloadClient" };

        Mock.SetupCommand(It.IsAny<RadarrApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<RadarrDownloadClientResourceDTO> { existingClient }))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiUpdateDownloadClientCommand>)
            .ReturnsAsync(Result.Ok(new RadarrDownloadClientResourceDTO { Id = 7 }))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiCreateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidNetworkSettings());
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendIntegrationSetupProgressAsync(It.IsAny<IntegrationSetupProgressDTO>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await sut.ExecuteAsync(
            new SetupRadarrDownloadClientCommand { IntegrationId = integration.Id },
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.DownloadClientId.ShouldBe(7);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenCreateDownloadClientFails()
    {
        // Arrange
        var integration = await SetupIntegrationAsync();
        Mock.SetupCommand(It.IsAny<RadarrApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<RadarrDownloadClientResourceDTO>()))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiCreateDownloadClientCommand>)
            .ReturnsAsync(Result.Fail("Create failed"))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiUpdateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidNetworkSettings());
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendIntegrationSetupProgressAsync(It.IsAny<IntegrationSetupProgressDTO>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await sut.ExecuteAsync(
            new SetupRadarrDownloadClientCommand { IntegrationId = integration.Id },
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldBuildResourceWithSslEnabled_WhenReverseProxyUrlIsHttps()
    {
        // Arrange
        var integration = await SetupIntegrationAsync();
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

        var sut = CreateSut(ValidNetworkSettings("https://reaparr.custom-domain.nl", string.Empty));
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendIntegrationSetupProgressAsync(It.IsAny<IntegrationSetupProgressDTO>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await sut.ExecuteAsync(
            new SetupRadarrDownloadClientCommand { IntegrationId = integration.Id },
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

        var sut = CreateSut(ValidNetworkSettings("http://reaparr.example.com:8080", "/reaparr"));
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendIntegrationSetupProgressAsync(It.IsAny<IntegrationSetupProgressDTO>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await sut.ExecuteAsync(
            new SetupRadarrDownloadClientCommand { IntegrationId = integration.Id },
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

        var sut = CreateSut(ValidNetworkSettings("not-a-url"));
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendIntegrationSetupProgressAsync(It.IsAny<IntegrationSetupProgressDTO>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await sut.ExecuteAsync(
            new SetupRadarrDownloadClientCommand { IntegrationId = integration.Id },
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
        portField.Value.ShouldBe(5000);

        var useSslField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "useSsl");
        useSslField.ShouldNotBeNull();
        useSslField.Value.ShouldBe(false);

        var urlBaseField = capturedResource.Fields!.FirstOrDefault(f => f.Name == "urlBase");
        urlBaseField.ShouldNotBeNull();
        urlBaseField.Value.ShouldBe($"api/public/integrations/{integration.Id}/download-client");

        Mock.Mock<ICommandExecutor>().Verify();
    }
}
