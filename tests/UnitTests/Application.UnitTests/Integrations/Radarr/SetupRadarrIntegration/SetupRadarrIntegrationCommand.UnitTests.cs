namespace Reaparr.Application.UnitTests;

public class SetupRadarrIntegrationCommandUnitTests : BaseCommandUnitTest<SetupRadarrIntegrationCommand>
{
    [Test]
    public async Task ShouldKeepRemoteIdsAndUnconfiguredState_WhenValidationFails()
    {
        // Arrange
        await SetupDatabase(55343, config => config.RadarrIntegrationCount = 1);
        var integration = await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken);
        await IDbContext.RadarrIntegrations.ExecuteUpdateAsync(
            x => x.SetProperty(p => p.ProvisioningState, IntegrationProvisioningState.Unconfigured),
            CancellationToken
        );
        var command = new SetupRadarrIntegrationCommand(integration.Id);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SetupRadarrDownloadClientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Ok(
                    new SetupRadarrDownloadClientCommandResult
                    {
                        DownloadClientId = 41,
                        Resource = new RadarrDownloadContractDTO(),
                    }
                )
            )
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SetupRadarrIndexerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Ok(
                    new SetupRadarrIndexerCommandResult { IndexerId = 42, Resource = new RadarrIndexerContractDTO() }
                )
            )
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ValidateRadarrIntegrationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Reaparr resource validation failed"))
            .Verifiable(Times.Once());
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendIntegrationSetupProgressAsync(It.IsAny<IntegrationSetupProgressDTO>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(5));

        // Act
        var result = await TestHandlerExecuteAsync<RadarrIntegration>(command);
        var updated = await IDbContext.RadarrIntegrations.AsNoTracking().SingleAsync(CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        updated.ExternalDownloadClientId.ShouldBe(41);
        updated.ExternalIndexerId.ShouldBe(42);
        updated.ProvisioningState.ShouldBe(IntegrationProvisioningState.Unconfigured);
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<IProgressHubService>().Verify();
    }
}
