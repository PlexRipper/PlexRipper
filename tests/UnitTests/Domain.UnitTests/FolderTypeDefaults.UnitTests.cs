namespace Reaparr.Domain.UnitTests;

public class FolderTypeDefaultsUnitTests : BaseUnitTest
{
    [Test]
    public void ShouldUseDownloadFolderTypeValue_AsDefaultDownloadFolderId()
    {
        // Arrange
        var expectedId = (int)FolderType.DownloadFolder;

        // Act
        var defaultDownloadFolderId = FolderTypeDefaults.DefaultDownloadFolderId;

        // Assert
        defaultDownloadFolderId.ShouldBe(expectedId);
        defaultDownloadFolderId.ShouldBe(1);
    }

    [Test]
    public void ShouldMapNoneMediaTypeToCanonicalDefaultDownloadFolderId()
    {
        // Arrange
        var expectedId = FolderTypeDefaults.DefaultDownloadFolderId;
        var mediaType = PlexMediaType.None;

        // Act
        var destinationFolderId = mediaType.ToDefaultDestinationFolderId();

        // Assert
        destinationFolderId.ShouldBe(expectedId);
    }
}
