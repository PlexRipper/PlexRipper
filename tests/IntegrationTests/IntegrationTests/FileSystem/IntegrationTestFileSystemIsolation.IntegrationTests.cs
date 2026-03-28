namespace Reaparr.IntegrationTests;

public class IntegrationTestFileSystemIsolationIntegrationTests : BaseIntegrationTests
{
    [Test]
    public async Task ShouldUseUniqueSandboxAndCleanupWhenContainerIsDisposed()
    {
        string firstSandboxPath;
        string secondSandboxPath;

        using (var firstContainer = await CreateContainer(new Seed(101)))
        {
            firstSandboxPath = firstContainer.TestFileSystemRootPath;
            Directory.Exists(firstSandboxPath).ShouldBeTrue();

            using (var secondContainer = await CreateContainer(new Seed(202)))
            {
                secondSandboxPath = secondContainer.TestFileSystemRootPath;
                secondSandboxPath.ShouldNotBe(firstSandboxPath);
                Directory.Exists(secondSandboxPath).ShouldBeTrue();
            }

            Directory.Exists(secondSandboxPath).ShouldBeFalse();
        }

        Directory.Exists(firstSandboxPath).ShouldBeFalse();
    }
}
