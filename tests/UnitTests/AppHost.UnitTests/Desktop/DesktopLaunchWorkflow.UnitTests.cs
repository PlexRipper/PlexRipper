using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Reaparr.Build;

namespace Reaparr.AppHost.UnitTests;

public class DesktopLaunchWorkflowUnitTests : BaseUnitTest<DesktopLaunchWorkflowUnitTests>
{
    [Test]
    public void ShouldMovePendingPackagesToHoldDirectory_WhenClearingPendingVelopackPackages()
    {
        // Arrange
        var packageRoot = "/tmp/velopack/Reaparr";
        var packagesDirectory = Path.Combine(packageRoot, "packages");
        var holdDirectory = Path.Combine(packageRoot, "packages-hold");
        var packagePath = Path.Combine(packagesDirectory, "Reaparr-1.2.3-linux-x64-full.nupkg");
        IFileSystem? fileSystem = null;

        SetupFileSystem(system =>
        {
            fileSystem = system;
            system.AddDirectory(packagesDirectory);
            system.AddFile(packagePath, new MockFileData("pending"));
            system.AddFile(Path.Combine(packagesDirectory, ".betaId"), new MockFileData("linux-x64-dev"));
            system.AddFile(Path.Combine(packagesDirectory, ".velopack_lock"), new MockFileData(string.Empty));
            system.AddDirectory(Path.Combine(packagesDirectory, "VelopackTemp"));
        });
        fileSystem.ShouldNotBeNull();

        // Act
        var movedPackages = DesktopLaunchWorkflow.ClearPendingVelopackPackages(packageRoot, fileSystem);

        // Assert
        movedPackages.Count.ShouldBe(1);
        movedPackages[0].ShouldBe(Path.Combine(holdDirectory, "Reaparr-1.2.3-linux-x64-full.nupkg"));
        fileSystem.File.Exists(packagePath).ShouldBeFalse();
        fileSystem.File.Exists(movedPackages[0]).ShouldBeTrue();
        fileSystem.File.Exists(Path.Combine(packagesDirectory, ".betaId")).ShouldBeTrue();
        fileSystem.File.Exists(Path.Combine(packagesDirectory, ".velopack_lock")).ShouldBeTrue();
        fileSystem.Directory.Exists(Path.Combine(packagesDirectory, "VelopackTemp")).ShouldBeTrue();
    }
}
