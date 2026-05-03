using Autofac;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Reaparr.Build.UnitTests;

public class FileSystemTasksUnitTests : BaseUnitTest
{
    [Test]
    public void ShouldThrow_WhenCopyTargetIsNestedInsideSource()
    {
        // Arrange
        const string source = "/repo/source";
        const string target = "/repo/source/nested-target";
        SetupFileSystem(system =>
        {
            system.AddDirectory(target);
            system.AddFile("/repo/source/source-file.txt", new MockFileData("source"));
        });

        var fileSystemTasks = CreateFileSystemTasks();

        // Act
        var action = () => fileSystemTasks.CopyDirectory(source, target);

        // Assert
        var exception = action.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("Refusing to copy");
        exception.Message.ShouldContain("nested target");
    }

    [Test]
    public void ShouldThrow_WhenClearingRepositoryRoot()
    {
        // Arrange
        const string root = "/repo";
        SetupFileSystem(system => system.AddDirectory(root));
        var fileSystemTasks = CreateFileSystemTasks();

        // Act
        var action = () => fileSystemTasks.ClearArtifactDirectory(root, root);

        // Assert
        var exception = action.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("repository root");
    }

    [Test]
    public void ShouldThrow_WhenClearingCustomDirectoryOutsideArtifactsRoot()
    {
        // Arrange
        const string root = "/repo";
        const string customDirectory = "/repo/Releases";
        SetupFileSystem(system => system.AddDirectory(customDirectory));
        var fileSystemTasks = CreateFileSystemTasks();

        // Act
        var action = () => fileSystemTasks.ClearArtifactDirectory(root, customDirectory);

        // Assert
        var exception = action.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("Refusing to clear custom artifact directory");
    }

    [Test]
    public void ShouldClearDirectory_WhenArtifactDirectoryIsUnderDefaultArtifactsRoot()
    {
        // Arrange
        const string root = "/repo";
        const string artifactDirectory = "/repo/.artifacts/linux-x64";
        SetupFileSystem(system =>
        {
            system.AddDirectory(artifactDirectory);
            system.AddFile("/repo/.artifacts/linux-x64/old-file.txt", new MockFileData("old"));
            system.AddDirectory("/repo/.artifacts/linux-x64/old-directory");
        });

        var fileSystem = Mock.Container.Resolve<IFileSystem>();
        var fileSystemTasks = CreateFileSystemTasks();

        // Act
        fileSystemTasks.ClearArtifactDirectory(root, artifactDirectory);

        // Assert
        fileSystem.Directory.Exists(artifactDirectory).ShouldBeTrue();
        fileSystem.Directory.EnumerateFileSystemEntries(artifactDirectory).ShouldBeEmpty();
    }

    private FileSystemTasks CreateFileSystemTasks() => new(Mock.Container.Resolve<IFileSystem>());
}
