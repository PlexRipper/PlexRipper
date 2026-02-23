using Autofac;
using FastEndpoints;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application.UnitTests;

public class SetupRadarrDownloadClientCommandUnitTests : BaseUnitTest<SetupRadarrDownloadClientCommandHandler>
{
    public SetupRadarrDownloadClientCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    // IRadarrSettings inherits IBaseSettingsModule<T> which has a static abstract member, making it
    // incompatible with Moq. Inject concrete RadarrSettings instances via TypedParameter instead.

    private SetupRadarrDownloadClientCommandHandler CreateSut(
        RadarrSettings radarrSettings,
        IntegrationsSettings integrationsSettings
    ) =>
        Mock.Create<SetupRadarrDownloadClientCommandHandler>(
            new TypedParameter(typeof(IRadarrSettings), radarrSettings),
            new TypedParameter(typeof(IIntegrationsSettings), integrationsSettings)
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

    [Fact]
    public async Task ShouldReturnFailedResult_WhenRadarrBaseUrlIsEmpty()
    {
        // Arrange
        var sut = CreateSut(ValidSettings(baseUrl: string.Empty), ValidIntegrationsSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupRadarrDownloadClientCommand(new Uri("http://localhost:7575")),
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

    [Fact]
    public async Task ShouldReturnFailedResult_WhenRadarrApiKeyIsEmpty()
    {
        // Arrange
        var sut = CreateSut(ValidSettings(apiKey: string.Empty), ValidIntegrationsSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupRadarrDownloadClientCommand(new Uri("http://localhost:7575")),
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

    [Fact]
    public async Task ShouldReturnFailedResult_WhenGetDownloadClientsFails()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<RadarrApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Fail("Radarr unreachable"))
            .Verifiable(Times.Once);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupRadarrDownloadClientCommand(new Uri("http://localhost:7575")),
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
        Mock.SetupCommand(It.IsAny<RadarrApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<RadarrDownloadClientResourceDTO>()))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiCreateDownloadClientCommand>)
            .ReturnsAsync(Result.Ok(new RadarrDownloadClientResourceDTO { Id = 42 }))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiUpdateDownloadClientCommand>).Verifiable(Times.Never);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupRadarrDownloadClientCommand(new Uri("http://localhost:7575")),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.DownloadClientId.ShouldBe(42);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Fact]
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

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupRadarrDownloadClientCommand(new Uri("http://localhost:7575")),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.DownloadClientId.ShouldBe(7);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenCreateDownloadClientFails()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<RadarrApiGetDownloadClientsCommand>)
            .ReturnsAsync(Result.Ok(new List<RadarrDownloadClientResourceDTO>()))
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<RadarrApiCreateDownloadClientCommand>)
            .ReturnsAsync(Result.Fail("Create failed"))
            .Verifiable(Times.Once);

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupRadarrDownloadClientCommand(new Uri("http://localhost:7575")),
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

        var sut = CreateSut(ValidSettings(), ValidIntegrationsSettings());

        // Act
        var result = await sut.ExecuteAsync(
            new SetupRadarrDownloadClientCommand(new Uri("https://radarr.custom-domain.nl")),
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
