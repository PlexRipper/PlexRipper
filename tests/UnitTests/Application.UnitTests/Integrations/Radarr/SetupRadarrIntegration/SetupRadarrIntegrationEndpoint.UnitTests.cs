namespace Reaparr.Application.UnitTests;

public class SetupRadarrIntegrationEndpointUnitTests
    : BaseEndpointUnitTest<
        SetupRadarrIntegrationEndpoint,
        SetupRadarrIntegrationRequest,
        ResultDTO<RadarrIntegrationDTO>
    >
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
        var request = new SetupRadarrIntegrationRequest { IntegrationId = integration.Id };

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
        var endpointResult = await TestEndpointHandleAsync(request);
        var updated = await IDbContext.RadarrIntegrations.AsNoTracking().SingleAsync(CancellationToken);

        // Assert
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        updated.ExternalDownloadClientId.ShouldBe(41);
        updated.ExternalIndexerId.ShouldBe(42);
        updated.ProvisioningState.ShouldBe(IntegrationProvisioningState.Unconfigured);
        Mock.Mock<ICommandExecutor>().Verify();
    }
}
