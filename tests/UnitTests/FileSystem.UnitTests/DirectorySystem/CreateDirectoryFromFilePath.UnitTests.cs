using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FileSystem.Contracts;
using MockFileSystem = System.IO.Abstractions.TestingHelpers.MockFileSystem;

namespace FileSystem.UnitTests.DirectorySystem;

public class CreateDirectoryFromFilePathUnitTests : BaseUnitTest<PlexRipper.FileSystem.DirectorySystem>
{
    public CreateDirectoryFromFilePathUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldReturnFailedResult_WhenFilePathIsEmpty()
    {
        // Act
        var filePath = string.Empty;
        var result = _sut.CreateDirectoryFromFilePath(filePath);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public void ShouldReturnSuccessResult_WhenFilePathIsValid()
    {
        // Arrange
        var filePath =
            "/mnt/DATA/PlexRipperCache/Downloads/TvShows/Reno 911!/Season 1/Reno 911! - S01E01 - How We Do It in Reno (Pilot) WEBDL-1080p.part1.mkv";
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { { filePath, new MockFileData("Testing is meh.") } }
        );

        mock.Mock<IPathSystem>()
            .Setup(x => x.GetDirectoryName(It.IsAny<string>()))
            .Returns(Result.Ok("/mnt/DATA/PlexRipperCache/Downloads/TvShows/Reno 911!/Season 1"));
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns((string path) => fileSystem.Directory.CreateDirectory(path));

        // Act
        var result = _sut.CreateDirectoryFromFilePath(filePath);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ShouldReturnFailedResult_WhenUnauthorizedAccessExceptionIsThrown()
    {
        // Arrange
        var filePath =
            "/mnt/DATA/PlexRipperCache/Downloads/TvShows/Reno 911!/Season 1/Reno 911! - S01E01 - How We Do It in Reno (Pilot) WEBDL-1080p.part1.mkv";

        mock.Mock<IPathSystem>()
            .Setup(x => x.GetDirectoryName(It.IsAny<string>()))
            .Returns(Result.Ok("/mnt/DATA/PlexRipperCache/Downloads/TvShows/Reno 911!/Season 1"));
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Throws(new UnauthorizedAccessException());

        // Act
        var result = _sut.CreateDirectoryFromFilePath(filePath);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailed.ShouldBeTrue();
        result.HasException<UnauthorizedAccessException>().ShouldBeTrue();
    }
}
