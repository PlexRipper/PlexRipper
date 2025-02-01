using System.IO.Abstractions;
using Autofac;
using ByteSizeLib;
using FileSystem.Contracts;

namespace FileSystem.UnitTests.FileSystemExtensions;

public class IFileSystemExtensionsUnitTests : BaseUnitTest
{
    public IFileSystemExtensionsUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldReturnCorrectAvailableSpace_WhenUsingTheRootPath()
    {
        // Arrange
        var path = "/";

        SetupFileSystem();

        // Act
        var sut = mock.Container.Resolve<IPath>();
        var result = sut.GetAvailableSpaceByDirectory(path);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe((long)ByteSize.FromGigaBytes(1000).Bytes);
    }
}
