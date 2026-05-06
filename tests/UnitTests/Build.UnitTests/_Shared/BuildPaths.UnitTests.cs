using System.IO.Abstractions;
using Autofac;

namespace Reaparr.Build.UnitTests;

public class BuildPathsUnitTests : BaseUnitTest
{
    [Test]
    public void ShouldReturnArtifactDirectoryUnderArtifactsRoot_WhenCalledWithArtifactsRoot()
    {
        // Arrange
        const string root = "/repo";
        const string runtimeIdentifier = "linux-x64";
        SetupFileSystem(system => system.AddDirectory(root));

        var fileSystem = Mock.Container.Resolve<IFileSystem>();
        var sut = new BuildPaths(root, fileSystem);

        // Act
        var result = sut.ArtifactDirectory(runtimeIdentifier);

        // Assert
        result.ShouldBe("/repo/.artifacts/linux-x64");
    }

    [Test]
    public void ShouldReturnPublishDirectoryUnderArtifactsDirectory_WhenCalledWithArtifactsDirectory()
    {
        // Arrange
        const string root = "/repo";
        const string runtimeIdentifier = "linux-x64";
        SetupFileSystem(system => system.AddDirectory(root));

        var fileSystem = Mock.Container.Resolve<IFileSystem>();
        var sut = new BuildPaths(root, fileSystem);

        // Act
        var result = sut.PublishDirectory(runtimeIdentifier);

        // Assert
        result.ShouldBe("/repo/.artifacts/linux-x64/publish");
    }
}
