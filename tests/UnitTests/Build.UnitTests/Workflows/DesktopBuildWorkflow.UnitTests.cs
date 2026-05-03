using System.IO.Abstractions.TestingHelpers;

namespace Reaparr.Build.UnitTests;

internal class DesktopRunBuildCommandHandlerUnitTests : BaseUnitTest<DesktopRunBuildCommandHandler>
{
    [Test]
    public async Task ShouldNotInvokePackageCommand_WhenSkipPackageIsTrue()
    {
        // Arrange
        SetupFileSystem(system =>
        {
            system.AddFile("/repo/Reaparr.sln", new MockFileData(string.Empty));
            system.AddDirectory("/repo/src/AppHost/ClientApp/.output/public");
            system.AddDirectory("/repo/.artifacts/linux-x64/publish");
            system.AddFile("/repo/.artifacts/linux-x64/publish/Reaparr.AppHost", new MockFileData("bin"));
        });

        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            Version = "1.2.3",
            InformationalVersion = "1.2.3-dev.1",
            SkipPackage = true,
            DryRun = true,
            SkipRestore = true,
        };

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DesktopPublishBuildCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(0))
            .Verifiable(Times.Once());

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DesktopPackageBuildCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(0))
            .Verifiable(Times.Never());

        // Act
        var result = await Sut.ExecuteAsync(new DesktopRunBuildCommand(settings), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(0);
        Mock.Mock<ICommandExecutor>().Verify();
    }
}
