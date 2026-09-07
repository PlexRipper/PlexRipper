namespace Reaparr.Application.UnitTests;

public class SetupSonarrDownloadClientPayloadUnitTests : BaseUnitTest<SetupSonarrDownloadClientCommandHandler>
{
    [Test]
    public async Task ShouldForceCreateWithPersistedQBittorrentApiKey()
    {
        // Arrange
        await SetupDatabase(62641, config => config.SonarrIntegrationCount = 1);
        var integration = await IDbContext.SonarrIntegrations.SingleAsync(CancellationToken);
        var expectedApiKey = integration.QBittorrentApiKey;
        SonarrApiCreateDownloadClientCommand? capturedCommand = null;
        var sut = Mock.Create<SetupSonarrDownloadClientCommandHandler>(
            new TypedParameter(typeof(INetworkSettings), NetworkSettingsModule.Create())
        );

        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<SonarApiGetDownloadClientsCommand>(command => command.IntegrationId == integration.Id),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok(new List<DownloadClientResourceDTO>()))
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SonarrApiCreateDownloadClientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (SonarrApiCreateDownloadClientCommand command, CancellationToken _) =>
                {
                    capturedCommand = command;
                    return Result.Ok(new SonarrDownloadContractDTO { Id = 41 });
                }
            )
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SonarApiUpdateDownloadClientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new SonarrDownloadContractDTO()))
            .Verifiable(Times.Never());
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
        capturedCommand.ShouldNotBeNull();
        capturedCommand.ForceSave.ShouldBeTrue();
        capturedCommand.Resource.Fields!.ShouldContain(x => x.Name == "apiKey" && Equals(x.Value, expectedApiKey));
        capturedCommand.Resource.Fields!.ShouldNotContain(x => x.Name == "apikey");
        result.Value.Resource.ShouldBeSameAs(capturedCommand.Resource);
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<IProgressHubService>().Verify();
    }
}
