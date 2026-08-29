namespace Reaparr.Application.UnitTests;

public class CreateRadarrIntegrationEndpointUnitTests
    : BaseEndpointUnitTest<
        CreateRadarrIntegrationEndpoint,
        CreateRadarrIntegrationRequest,
        ResultDTO<RadarrIntegrationDTO>
    >
{
    [Test]
    public async Task ShouldCreateNormalizedIntegration_WhenRequestIsUnique()
    {
        // Arrange
        await SetupDatabase(55301);
        var request = new CreateRadarrIntegrationRequest
        {
            Name = " Radarr Main ",
            Url = " https://radarr.example.com/ ",
            ApiKey = " radarr-key ",
            Category = " reaparr-movies ",
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response!;
        var integration = await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken);

        // Assert
        endpointResult.IsValid.ShouldBeTrue();
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(integration.Id);
        result.Value.ApiKey.ShouldBe("radarr-key");
        integration.DisplayName.ShouldBe("Radarr Main");
        integration.BaseUrl.ShouldBe("https://radarr.example.com");
        integration.Category.ShouldBe("reaparr-movies");
        integration.DownloadFolderId.ShouldBeNull();
        integration.ProvisioningState.ShouldBe(IntegrationProvisioningState.Unconfigured);
        integration.QBittorrentApiKey.ShouldStartWith("qbt_");
        integration.QBittorrentApiKey.Length.ShouldBe(32);
        integration.TorznabApiKey.Length.ShouldBe(32);
    }

    [Test]
    public async Task ShouldReturnBadRequest_WhenIntegrationConflicts()
    {
        // Arrange
        await SetupDatabase(55302);
        IDbContext.RadarrIntegrations.Add(
            new RadarrIntegration
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000055302"),
                DisplayName = "Existing Radarr",
                BaseUrl = "https://existing-radarr.example.com",
                RadarrApiKey = "existing-key",
                QBittorrentApiKey = IntegrationApiKeyGenerator.GenerateQBittorrentApiKey(),
                TorznabApiKey = IntegrationApiKeyGenerator.GenerateTorznabApiKey(),
                Category = "reaparr-movies",
                ProvisioningState = IntegrationProvisioningState.Unconfigured,
            }
        );
        await IDbContext.SaveChangesAsync(CancellationToken);
        var request = new CreateRadarrIntegrationRequest
        {
            Name = "New Radarr",
            Url = "https://new-radarr.example.com",
            ApiKey = "new-key",
            Category = " reaparr-movies ",
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response!;

        // Assert
        endpointResult.IsValid.ShouldBeTrue();
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        (await IDbContext.RadarrIntegrations.CountAsync(CancellationToken)).ShouldBe(1);
    }
}
