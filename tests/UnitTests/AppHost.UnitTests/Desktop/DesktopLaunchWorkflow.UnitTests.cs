using Reaparr.Build;

namespace Reaparr.AppHost.UnitTests;

public class DesktopLaunchWorkflowUnitTests
{
    [Test]
    public void ShouldMovePendingPackagesToHoldDirectory_WhenClearingPendingVelopackPackages()
    {
        // Arrange
        var packageRoot = Path.Combine(Path.GetTempPath(), $"reaparr-launch-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(packageRoot);
        var packagesDirectory = Path.Combine(packageRoot, "packages");
        Directory.CreateDirectory(packagesDirectory);
        var holdDirectory = Path.Combine(packageRoot, "packages-hold");
        var packagePath = Path.Combine(packagesDirectory, "Reaparr-1.2.3-linux-x64-full.nupkg");
        File.WriteAllText(packagePath, "pending");
        File.WriteAllText(Path.Combine(packagesDirectory, ".betaId"), "linux-x64-dev");
        File.WriteAllText(Path.Combine(packagesDirectory, ".velopack_lock"), string.Empty);
        Directory.CreateDirectory(Path.Combine(packagesDirectory, "VelopackTemp"));

        try
        {
            // Act
            var movedPackages = ClearPendingVelopackPackages(packageRoot);

            // Assert
            movedPackages.Count.ShouldBe(1);
            movedPackages[0].ShouldBe(Path.Combine(holdDirectory, "Reaparr-1.2.3-linux-x64-full.nupkg"));
            File.Exists(packagePath).ShouldBeFalse();
            File.Exists(movedPackages[0]).ShouldBeTrue();
            File.Exists(Path.Combine(packagesDirectory, ".betaId")).ShouldBeTrue();
            File.Exists(Path.Combine(packagesDirectory, ".velopack_lock")).ShouldBeTrue();
            Directory.Exists(Path.Combine(packagesDirectory, "VelopackTemp")).ShouldBeTrue();
        }
        finally
        {
            if (Directory.Exists(packageRoot))
                Directory.Delete(packageRoot, recursive: true);
        }
    }

    private static IReadOnlyList<string> ClearPendingVelopackPackages(string packageRoot)
    {
        IReadOnlyList<string> movedPackages = DesktopLaunchWorkflow.ClearPendingVelopackPackages(packageRoot);
        movedPackages.ShouldNotBeNull();
        return movedPackages;
    }
}
