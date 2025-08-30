using System.IO.Abstractions;
using Autofac;
using Reaparr.BaseTests;
using Reaparr.Environment;
using Reaparr.FileSystem.Contracts;

namespace Reaparr.FileSystem.UnitTests.FileSystemExtensions;

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
        result.Value.ShouldBe(DefaultAvailableSpace);
    }

    [Fact]
    public void ShouldReturnCorrectAvailableSpace_WhenUsingTheMoviesPath()
    {
        // Arrange
        var path = PathProvider.DefaultMovieDestinationFolder;

        SetupFileSystem(system =>
        {
            system.AddDirectory(path);
        });

        // Act
        var sut = mock.Container.Resolve<IPath>();
        var result = sut.GetAvailableSpaceByDirectory(path);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(DefaultAvailableSpace);
    }

    [Fact]
    public void ShouldReturnCorrectAvailableSpace_WhenUsingACustomFolder()
    {
        // Arrange
        var path = "/SomeCustomFolder";

        SetupFileSystem(system =>
        {
            system.AddDirectory(path);
        });

        // Act
        var sut = mock.Container.Resolve<IPath>();
        var result = sut.GetAvailableSpaceByDirectory(path);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(DefaultAvailableSpace);
    }

    [Fact]
    public void ShouldReturnFailedResult_WhenUsingAFolderThatDoesNotExist()
    {
        // Arrange
        var path = @"C:\FolderDoesNotExist";

        SetupFileSystem();

        // Act
        var sut = mock.Container.Resolve<IPath>();
        var result = sut.GetAvailableSpaceByDirectory(path);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
    }
}
