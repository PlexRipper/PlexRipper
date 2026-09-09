namespace Reaparr.Application.UnitTests;

public class UpdateSonarrIntegrationEndpointUnitTests
    : BaseEndpointUnitTest<
        UpdateSonarrIntegrationEndpoint,
        UpdateSonarrIntegrationRequest,
        ResultDTO<SonarrIntegrationDTO>
    >
{
    [Test]
    public async Task ShouldUpdateIntegrationAndMarkChangesPending_WhenCategoryChanges()
    {
        // Arrange
        await SetupDatabase(62603);
        var dbContext = IDbContext;
        var integration = new SonarrIntegration
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000062603"),
            DisplayName = "Sonarr",
            BaseUrl = "https://sonarr.example.com",
            SonarrApiKey = "old-key",
            QBittorrentApiKey = "qbt_23456789ABCDEFGHIJKLMNPQ",
            TorznabApiKey = "0123456789abcdef0123456789abcdef",
            Category = "old-category",
            DownloadFolderId = FolderTypeDefaults.DefaultDownloadFolderId,
            ProvisioningState = IntegrationProvisioningState.Configured,
        };
        dbContext.SonarrIntegrations.Add(integration);
        await dbContext.SaveChangesAsync(CancellationToken);
        var request = new UpdateSonarrIntegrationRequest
        {
            IntegrationId = integration.Id,
            Name = " Updated Sonarr ",
            Url = " https://updated-sonarr.example.com/ ",
            ApiKey = " new-key ",
            Category = " new-category ",
            DownloadFolderId = FolderTypeDefaults.DefaultDownloadFolderId,
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response!;
        var updated = await dbContext.SonarrIntegrations.SingleAsync(CancellationToken);

        // Assert
        endpointResult.IsValid.ShouldBeTrue();
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ApiKey.ShouldBe("new-key");
        updated.DisplayName.ShouldBe("Updated Sonarr");
        updated.BaseUrl.ShouldBe("https://updated-sonarr.example.com");
        updated.Category.ShouldBe("new-category");
        updated.DownloadFolderId.ShouldBe(FolderTypeDefaults.DefaultDownloadFolderId);
        updated.ProvisioningState.ShouldBe(IntegrationProvisioningState.ChangesPending);
        updated.QBittorrentApiKey.ShouldBe("qbt_23456789ABCDEFGHIJKLMNPQ");
        updated.TorznabApiKey.ShouldBe("0123456789abcdef0123456789abcdef");
    }

    [Test]
    public async Task ShouldReturnNotFound_WhenIntegrationDoesNotExist()
    {
        // Arrange
        await SetupDatabase(62604);
        var request = new UpdateSonarrIntegrationRequest
        {
            IntegrationId = Guid.Parse("00000000-0000-0000-0000-000000062604"),
            Name = "Missing Sonarr",
            Url = "https://missing-sonarr.example.com",
            ApiKey = "missing-key",
            Category = "missing-category",
            DownloadFolderId = FolderTypeDefaults.DefaultDownloadFolderId,
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response!;

        // Assert
        endpointResult.IsValid.ShouldBeTrue();
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        (await IDbContext.SonarrIntegrations.CountAsync(CancellationToken)).ShouldBe(0);
    }
}
