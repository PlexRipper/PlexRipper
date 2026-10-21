using System.Text.Json;

namespace Reaparr.Domain.UnitTests;

public class PhotoAssetKindUnitTests : BaseUnitTest
{
    [Test]
    [Arguments(PhotoAssetKind.Image, "Image")]
    [Arguments(PhotoAssetKind.VideoClip, "VideoClip")]
    public void ShouldSerializeDocumentedPhotoKindNames_WhenWritingApiJson(PhotoAssetKind kind, string expectedName)
    {
        // Arrange
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        // Act
        var json = JsonSerializer.Serialize(new { AssetKind = kind }, options);

        // Assert
        json.ShouldBe($"{{\"assetKind\":\"{expectedName}\"}}");
    }

    [Test]
    [Arguments("Image", PhotoAssetKind.Image)]
    [Arguments("VideoClip", PhotoAssetKind.VideoClip)]
    public void ShouldReadDocumentedPhotoKindNames_WhenReadingApiJson(string name, PhotoAssetKind expectedKind)
    {
        // Arrange
        var json = $"\"{name}\"";
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        // Act
        var kind = JsonSerializer.Deserialize<PhotoAssetKind>(json, options);

        // Assert
        kind.ShouldBe(expectedKind);
    }
}
