namespace Reaparr.Application.UnitTests;

public class UpdateRadarrIntegrationEndpointUnitTests
    : BaseEndpointUnitTest<
        UpdateRadarrIntegrationEndpoint,
        UpdateRadarrIntegrationRequest,
        ResultDTO<RadarrIntegrationDTO>
    >
{
    [Test]
    public async Task ShouldUpdateIntegrationAndMarkChangesPending_WhenCategoryChanges()
    {
        // Arrange
        await SetupDatabase(55303);
        var dbContext = IDbContext;
        var integration = new RadarrIntegration
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000055303"),
            DisplayName = "Radarr",
            BaseUrl = "https://radarr.example.com",
            RadarrApiKey = "old-key",
            QBittorrentApiKey = "qbt_23456789ABCDEFGHIJKLMNPQ",
            TorznabApiKey = "0123456789abcdef0123456789abcdef",
            Category = "old-category",
            DownloadFolderId = PlexMediaType.None.ToDefaultDestinationFolderId(),
            ProvisioningState = IntegrationProvisioningState.Configured,
        };
        dbContext.RadarrIntegrations.Add(integration);
        await dbContext.SaveChangesAsync(CancellationToken);
        var request = new UpdateRadarrIntegrationRequest
        {
            IntegrationId = integration.Id,
            Name = " Updated Radarr ",
            Url = " https://updated-radarr.example.com/ ",
            ApiKey = " new-key ",
            Category = " new-category ",
            DownloadFolderId = PlexMediaType.None.ToDefaultDestinationFolderId(),
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response!;
        var updated = await dbContext.RadarrIntegrations.SingleAsync(CancellationToken);

        // Assert
        endpointResult.IsValid.ShouldBeTrue();
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ApiKey.ShouldBe("new-key");
        updated.DisplayName.ShouldBe("Updated Radarr");
        updated.BaseUrl.ShouldBe("https://updated-radarr.example.com");
        updated.Category.ShouldBe("new-category");
        updated.DownloadFolderId.ShouldBe(PlexMediaType.None.ToDefaultDestinationFolderId());
        updated.ProvisioningState.ShouldBe(IntegrationProvisioningState.ChangesPending);
        updated.QBittorrentApiKey.ShouldBe("qbt_23456789ABCDEFGHIJKLMNPQ");
        updated.TorznabApiKey.ShouldBe("0123456789abcdef0123456789abcdef");
    }

    [Test]
    public async Task ShouldReturnNotFound_WhenIntegrationDoesNotExist()
    {
        // Arrange
        await SetupDatabase(55304);
        var request = new UpdateRadarrIntegrationRequest
        {
            IntegrationId = Guid.Parse("00000000-0000-0000-0000-000000055304"),
            Name = "Missing Radarr",
            Url = "https://missing-radarr.example.com",
            ApiKey = "missing-key",
            Category = "missing-category",
            DownloadFolderId = PlexMediaType.None.ToDefaultDestinationFolderId(),
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response!;

        // Assert
        endpointResult.IsValid.ShouldBeTrue();
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        (await IDbContext.RadarrIntegrations.CountAsync(CancellationToken)).ShouldBe(0);
    }
}
