using Reaparr.FileSystem.Contracts;

namespace Reaparr.FileSystem.UnitTests;

public class FileInfoMappersUnitTests
{
    [Test]
    public void ShouldSetReadPermissionToTrue_WhenFileIsWritable()
    {
        // Arrange
        var tempFilePath = Path.GetTempFileName();

        try
        {
            var fileInfo = new FileInfo(tempFilePath);

            // Act
            var result = fileInfo.ToModel();

            // Assert
            result.HasReadPermission.ShouldBeTrue();
            result.HasWritePermission.ShouldBeTrue();
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }
}
