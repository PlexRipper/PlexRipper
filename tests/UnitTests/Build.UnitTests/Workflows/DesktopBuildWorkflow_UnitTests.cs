using Autofac;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO.Abstractions;

namespace Reaparr.Build.UnitTests;

public class DesktopBuildWorkflowUnitTests : BaseUnitTest
{
    [Test]
    public void ShouldThrowBeforePublishing_WhenLinuxLaunchModeIsUnsupportedAndPackagingIsEnabled()
    {
        // Arrange
        const string root = "/repo";
        SetupFileSystem(system =>
        {
            system.AddFile("/repo/Reaparr.sln", string.Empty);
            system.AddDirectory("/repo/src/AppHost/ClientApp/.output/public");
        });

        var fileSystem = Mock.Container.Resolve<IFileSystem>();
        var paths = new BuildPaths(root, fileSystem);
        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            Version = "1.2.3",
            InformationalVersion = "1.2.3-dev.1",
            SkipFrontend = true,
            DryRun = true,
            LaunchMode = "invalid",
        };

        var workflow = DesktopBuildWorkflow.Create(paths, settings, NullLoggerFactory.Instance, fileSystem);

        // Act
        var action = () => workflow.RunAsync();

        // Assert
        var exception = action.ShouldThrow<ArgumentException>();
        exception.Message.ShouldContain("Unsupported launch mode 'invalid'");
        exception.ParamName.ShouldBe(nameof(DesktopCommandSettings.LaunchMode));
    }
}
