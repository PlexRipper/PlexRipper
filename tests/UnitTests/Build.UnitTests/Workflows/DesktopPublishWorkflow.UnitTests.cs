using System.IO.Abstractions.TestingHelpers;
using Autofac;

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

        // DesktopPublishBuildCommandHandler resolves BuildPaths from DI. BuildPaths requires a
        // rootDirectory string constructor parameter, so tests must register it explicitly.
        // /repo matches the mocked filesystem layout used by this test suite.
        SetupDependencies(builder =>
            builder.Register(_ => "/repo")
                .As<string>()
                .SingleInstance()
        );

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
            DryRun = false,
            SkipRestore = true,
        };

        SetupDependencies(builder =>
            builder.Register(_ => "/repo")
                .As<string>()
                .SingleInstance()
        );

        Mock.Mock<IDesktopCommandRunner>()
            .Setup(x => x.RequireCommandAsync("dotnet"))
            .Returns(Task.CompletedTask);

        Mock.Mock<IDesktopCommandRunner>()
            .Setup(x => x.RunCommandAsync("dotnet", It.IsAny<IReadOnlyList<string>>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

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
        Mock.Mock<IDesktopCommandRunner>().Verify(x => x.RequireCommandAsync("dotnet"), Times.Once());
        Mock.Mock<IDesktopCommandRunner>()
            .Verify(x => x.RunCommandAsync("dotnet", It.IsAny<IReadOnlyList<string>>(), It.IsAny<string?>()), Times.Once());
        Mock.Mock<IDesktopCommandRunner>().Verify();
    }
}
