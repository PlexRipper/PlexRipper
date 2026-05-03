using System.IO.Abstractions.TestingHelpers;

namespace Reaparr.Build.UnitTests;

internal class DesktopPublishWorkflowUnitTests : BaseUnitTest<DesktopPublishBuildCommandHandler>
{
    [Test]
    public async Task ShouldFail_WhenVersionIsMissing()
    {
        // Arrange
        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            InformationalVersion = "1.2.3-dev.1",
            DryRun = true,
        };

        Mock.Mock<IDesktopCommandRunner>()
            .Setup(x => x.RequireCommandAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never());

        Mock.Mock<IDesktopCommandRunner>()
            .Setup(x => x.RunCommandAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never());

        // Act
        var result = await Sut.ExecuteAsync(new DesktopPublishBuildCommand(settings), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<IDesktopCommandRunner>().Verify();
    }

    [Test]
    public async Task ShouldReuseExistingFrontendOutput_WhenFrontendPublicDirectoryAlreadyExists()
    {
        // Arrange
        SetupFileSystem(system =>
        {
            system.AddDirectory("/repo/src/AppHost/ClientApp/.output/public");
            system.AddFile("/repo/src/AppHost/ClientApp/.output/public/index.html", new MockFileData("frontend"));
        });

        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            Version = "1.2.3",
            InformationalVersion = "1.2.3-dev.1",
            DryRun = true,
            SkipRestore = true,
        };

        Mock.Mock<IDesktopCommandRunner>()
            .Setup(x => x.RequireCommandAsync("bun"))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never());

        Mock.Mock<IDesktopCommandRunner>()
            .Setup(x => x.RunCommandAsync("bun", It.IsAny<IReadOnlyList<string>>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never());

        // Act
        var result = await Sut.ExecuteAsync(new DesktopPublishBuildCommand(settings), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(0);
        Mock.Mock<IDesktopCommandRunner>().Verify();
    }
}
