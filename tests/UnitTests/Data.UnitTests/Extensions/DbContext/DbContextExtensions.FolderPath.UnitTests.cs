namespace Reaparr.Data.UnitTests;

public class DbContextExtensionsFolderPathUnitTests : BaseUnitTest
{
    [Test]
    public async Task ShouldReturnSeededDefaultDownloadFolder_WhenNoIntegrationIsProvided()
    {
        // Arrange
        await SetupDatabase(65701);

        // Act
        var downloadFolder = await IDbContext.GetDownloadFolder();

        // Assert
        downloadFolder.Id.ShouldBe(FolderTypeDefaults.DefaultDownloadFolderId);
        downloadFolder.FolderType.ShouldBe(FolderType.DownloadFolder);
        downloadFolder.MediaType.ShouldBe(PlexMediaType.None);
    }
}
