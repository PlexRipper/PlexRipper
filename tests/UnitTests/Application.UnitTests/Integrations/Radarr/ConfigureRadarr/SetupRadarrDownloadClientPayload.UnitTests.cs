namespace Reaparr.Application.UnitTests;

public class SetupRadarrDownloadClientPayloadUnitTests : BaseUnitTest<SetupRadarrDownloadClientCommandHandler>
{
    [Test]
    public async Task ShouldForceCreateWithPersistedQBittorrentApiKey()
    {
        // Arrange
        await SetupDatabase(55341, config => config.RadarrIntegrationCount = 1);
        var integration = await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken);
        var expectedApiKey = integration.QBittorrentApiKey;
        RadarrApiCreateDownloadClientCommand? capturedCommand = null;
        var sut = Mock.Create<SetupRadarrDownloadClientCommandHandler>(
            new TypedParameter(typeof(INetworkSettings), NetworkSettingsModule.Create())
        );

        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<RadarrApiGetDownloadClientsCommand>(command => command.IntegrationId == integration.Id),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok(new List<RadarrDownloadClientResourceDTO>()))
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<RadarrApiCreateDownloadClientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (RadarrApiCreateDownloadClientCommand command, CancellationToken _) =>
                {
                    capturedCommand = command;
                    return Result.Ok(new RadarrDownloadClientResourceDTO { Id = 41 });
                }
            )
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<RadarrApiUpdateDownloadClientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new RadarrDownloadClientResourceDTO()))
            .Verifiable(Times.Never());
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
        capturedCommand.ShouldNotBeNull();
        capturedCommand.ForceSave.ShouldBeTrue();
        capturedCommand.Resource.Fields!.ShouldContain(x => x.Name == "apiKey" && Equals(x.Value, expectedApiKey));
        capturedCommand.Resource.Fields!.ShouldNotContain(x => x.Name == "apikey");
        result.Value.Resource.ShouldBeSameAs(capturedCommand.Resource);
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<IProgressHubService>().Verify();
    }
}
