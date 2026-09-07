namespace Reaparr.Application.UnitTests;

public class CreateSonarrIntegrationEndpointUnitTests
    : BaseEndpointUnitTest<
        CreateSonarrIntegrationEndpoint,
        CreateSonarrIntegrationRequest,
        ResultDTO<SonarrIntegrationDTO>
    >
{
    [Test]
    public async Task ShouldCreateNormalizedIntegration_WhenRequestIsUnique()
    {
        // Arrange
        await SetupDatabase(62601);
        var request = new CreateSonarrIntegrationRequest
        {
            Name = " Sonarr Main ",
            Url = " https://sonarr.example.com/ ",
            ApiKey = " sonarr-key ",
            Category = " reaparr-tv ",
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response!;
        var integration = await IDbContext.SonarrIntegrations.SingleAsync(CancellationToken);

        // Assert
        endpointResult.IsValid.ShouldBeTrue();
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(integration.Id);
        result.Value.ApiKey.ShouldBe("sonarr-key");
        integration.DisplayName.ShouldBe("Sonarr Main");
        integration.BaseUrl.ShouldBe("https://sonarr.example.com");
        integration.Category.ShouldBe("reaparr-tv");
        integration.DownloadFolderId.ShouldBeNull();
        integration.ProvisioningState.ShouldBe(IntegrationProvisioningState.Unconfigured);
        integration.QBittorrentApiKey.ShouldStartWith("qbt_");
        integration.QBittorrentApiKey.Length.ShouldBe(32);
        integration.TorznabApiKey.Length.ShouldBe(32);
        integration.TorznabApiKey.ShouldNotBe(integration.QBittorrentApiKey);
    }

    [Test]
    public async Task ShouldReturnBadRequest_WhenIntegrationConflicts()
    {
        // Arrange
        await SetupDatabase(62602);
        IDbContext.SonarrIntegrations.Add(
            new SonarrIntegration
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000062602"),
                DisplayName = "Existing Sonarr",
                BaseUrl = "https://existing-sonarr.example.com",
                SonarrApiKey = "existing-key",
                QBittorrentApiKey = IntegrationApiKeyGenerator.GenerateQBittorrentApiKey(),
                TorznabApiKey = IntegrationApiKeyGenerator.GenerateTorznabApiKey(),
                Category = "reaparr-tv",
                ProvisioningState = IntegrationProvisioningState.Unconfigured,
            }
        );
        await IDbContext.SaveChangesAsync(CancellationToken);
        var request = new CreateSonarrIntegrationRequest
        {
            Name = "New Sonarr",
            Url = "https://new-sonarr.example.com",
            ApiKey = "new-key",
            Category = " reaparr-tv ",
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response!;

        // Assert
        endpointResult.IsValid.ShouldBeTrue();
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        (await IDbContext.SonarrIntegrations.CountAsync(CancellationToken)).ShouldBe(1);
    }
}
