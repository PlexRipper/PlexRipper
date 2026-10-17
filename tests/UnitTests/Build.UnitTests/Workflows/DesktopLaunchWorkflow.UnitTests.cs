using System.IO.Abstractions.TestingHelpers;
using Autofac;

namespace Reaparr.Build.UnitTests;

internal class DesktopLaunchWorkflowUnitTests : BaseUnitTest<DesktopLaunchBuildCommandHandler>
{
    [Test]
    public async Task ShouldLaunchNewestTopLevelAppImage_WhenLinuxPackagedLaunchIsRequested()
    {
        // Arrange
        const string artifactDirectory = "/repo/release-output";
        const string olderArtifact = "/repo/release-output/Reaparr-1.0.0.AppImage";
        const string newestArtifact = "/repo/release-output/Reaparr-1.1.0.AppImage";
        const string nestedArtifact = "/repo/release-output/nested/Reaparr-nested.AppImage";
        var olderWriteTime = new DateTimeOffset(2026, 10, 16, 12, 0, 0, TimeSpan.Zero);
        var newestWriteTime = new DateTimeOffset(2026, 10, 17, 12, 0, 0, TimeSpan.Zero);

        SetupFileSystem(system =>
        {
            system.AddDirectory(artifactDirectory);
            system.AddDirectory("/repo/release-output/nested");
            system.AddFile(olderArtifact, new MockFileData("older") { LastWriteTime = olderWriteTime });
            system.AddFile(newestArtifact, new MockFileData("newest") { LastWriteTime = newestWriteTime });
            system.AddFile(nestedArtifact, new MockFileData("nested") { LastWriteTime = newestWriteTime.AddDays(1) });
            system.AddFile("/repo/release-output/not-an-app-image.zip", new MockFileData("other"));
        });

        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            LaunchMode = "packaged",
            ArtifactDirectory = "release-output",
        };

        SetupDependencies(builder => builder.Register(_ => "/repo").As<string>().SingleInstance());

        Mock.Mock<IDesktopCommandRunner>()
            .Setup(x =>
                x.ExecuteCommandAsync(
                    "chmod",
                    It.Is<IReadOnlyList<string>>(arguments =>
                        arguments.Count == 2 && arguments[0] == "+x" && arguments[1] == newestArtifact
                    ),
                    null
                )
            )
            .ReturnsAsync(0)
            .Verifiable(Times.Once());

        Mock.Mock<IDesktopCommandRunner>()
            .Setup(x =>
                x.ExecuteCommandAsync(
                    newestArtifact,
                    It.Is<IReadOnlyList<string>>(arguments => arguments.Count == 0),
                    null
                )
            )
            .ReturnsAsync(17)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(new DesktopLaunchBuildCommand(settings), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        result.Value.ShouldBe(17);
        Mock.Mock<IDesktopCommandRunner>().Verify();
    }

    [Test]
    public async Task ShouldFail_WhenLinuxPackagedArtifactIsMissing()
    {
        // Arrange
        const string artifactDirectory = "/repo/.artifacts/linux-x64";
        SetupFileSystem(system =>
        {
            system.AddDirectory(artifactDirectory);
            system.AddDirectory("/repo/.artifacts/linux-x64/nested");
            system.AddFile("/repo/.artifacts/linux-x64/nested/Reaparr.AppImage", new MockFileData("nested"));
            system.AddFile("/repo/.artifacts/linux-x64/Reaparr.zip", new MockFileData("not an AppImage"));
        });

        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            LaunchMode = "packaged",
        };

        SetupDependencies(builder => builder.Register(_ => "/repo").As<string>().SingleInstance());

        // Act
        var result = await Sut.ExecuteAsync(new DesktopLaunchBuildCommand(settings), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].Message.ShouldContain("No packaged Linux artifact");
    }

    [Test]
    public async Task ShouldFail_WhenLinuxPackagedArtifactDirectoryIsMissing()
    {
        // Arrange
        SetupFileSystem();

        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            LaunchMode = "packaged",
        };

        SetupDependencies(builder => builder.Register(_ => "/repo").As<string>().SingleInstance());

        // Act
        var result = await Sut.ExecuteAsync(new DesktopLaunchBuildCommand(settings), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].Message.ShouldContain("artifact directory");
    }

    [Test]
    public async Task ShouldFailAndNotLaunch_WhenChmodFailsForLinuxPackagedArtifact()
    {
        // Arrange
        const string artifactDirectory = "/repo/.artifacts/linux-x64";
        const string appImage = "/repo/.artifacts/linux-x64/Reaparr.AppImage";
        SetupFileSystem(system =>
        {
            system.AddDirectory(artifactDirectory);
            system.AddFile(appImage, new MockFileData("app image"));
        });

        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            LaunchMode = "packaged",
        };

        SetupDependencies(builder => builder.Register(_ => "/repo").As<string>().SingleInstance());

        Mock.Mock<IDesktopCommandRunner>()
            .Setup(x =>
                x.ExecuteCommandAsync(
                    "chmod",
                    It.Is<IReadOnlyList<string>>(arguments =>
                        arguments.Count == 2 && arguments[0] == "+x" && arguments[1] == appImage
                    ),
                    null
                )
            )
            .ReturnsAsync(9)
            .Verifiable(Times.Once());

        Mock.Mock<IDesktopCommandRunner>()
            .Setup(x =>
                x.ExecuteCommandAsync(
                    appImage,
                    It.Is<IReadOnlyList<string>>(arguments => arguments.Count == 0),
                    null
                )
            )
            .ReturnsAsync(0)
            .Verifiable(Times.Never());

        // Act
        var result = await Sut.ExecuteAsync(new DesktopLaunchBuildCommand(settings), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].Message.ShouldContain("chmod exited with code 9");
        Mock.Mock<IDesktopCommandRunner>().Verify();
    }

    [Test]
    public async Task ShouldLaunchPublishedExecutable_WhenLinuxLaunchModeIsPublished()
    {
        // Arrange
        const string publishedExecutable = "/repo/.artifacts/linux-x64/publish/Reaparr.AppHost";
        SetupFileSystem(system =>
        {
            system.AddDirectory("/repo/.artifacts/linux-x64/publish");
            system.AddFile(publishedExecutable, new MockFileData("published"));
        });

        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            LaunchMode = "published",
        };

        SetupDependencies(builder => builder.Register(_ => "/repo").As<string>().SingleInstance());

        Mock.Mock<IDesktopCommandRunner>()
            .Setup(x =>
                x.ExecuteCommandAsync(
                    publishedExecutable,
                    It.Is<IReadOnlyList<string>>(arguments => arguments.Count == 0),
                    null
                )
            )
            .ReturnsAsync(23)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(new DesktopLaunchBuildCommand(settings), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        result.Value.ShouldBe(23);
        Mock.Mock<IDesktopCommandRunner>().Verify();
    }

    [Test]
    public async Task ShouldLaunchPublishedExecutable_WhenLinuxPackagingIsSkipped()
    {
        // Arrange
        const string publishedExecutable = "/repo/.artifacts/linux-x64/publish/Reaparr.AppHost";
        SetupFileSystem(system =>
        {
            system.AddDirectory("/repo/.artifacts/linux-x64/publish");
            system.AddFile(publishedExecutable, new MockFileData("published"));
        });

        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            LaunchMode = "packaged",
            SkipPackage = true,
        };

        SetupDependencies(builder => builder.Register(_ => "/repo").As<string>().SingleInstance());

        Mock.Mock<IDesktopCommandRunner>()
            .Setup(x =>
                x.ExecuteCommandAsync(
                    publishedExecutable,
                    It.Is<IReadOnlyList<string>>(arguments => arguments.Count == 0),
                    null
                )
            )
            .ReturnsAsync(29)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(new DesktopLaunchBuildCommand(settings), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        result.Value.ShouldBe(29);
        Mock.Mock<IDesktopCommandRunner>().Verify();
    }

    [Test]
    public async Task ShouldUseExpectedWindowsLaunchFallback_OnEachHost()
    {
        // Arrange
        const string publishedExecutable = "/repo/.artifacts/win-x64/publish/Reaparr.AppHost.exe";
        SetupFileSystem(system =>
        {
            system.AddDirectory("/repo/.artifacts/win-x64/publish");
            system.AddFile(publishedExecutable, new MockFileData("published"));
        });

        var settings = new DesktopCommandSettings { RuntimeIdentifier = "win-x64" };
        SetupDependencies(builder => builder.Register(_ => "/repo").As<string>().SingleInstance());

        if (OperatingSystem.IsWindows())
        {
            Mock.Mock<IDesktopCommandRunner>()
                .Setup(x =>
                    x.ExecuteCommandAsync(
                        publishedExecutable,
                        It.Is<IReadOnlyList<string>>(arguments => arguments.Count == 0),
                        null
                    )
                )
                .ReturnsAsync(31)
                .Verifiable(Times.Once());
        }
        else
        {
            Mock.Mock<IDesktopCommandRunner>()
                .Setup(x => x.CommandExistsAsync("wine"))
                .ReturnsAsync(true)
                .Verifiable(Times.Once());

            Mock.Mock<IDesktopCommandRunner>()
                .Setup(x =>
                    x.ExecuteCommandAsync(
                        "wine",
                        It.Is<IReadOnlyList<string>>(arguments =>
                            arguments.Count == 1 && arguments[0] == publishedExecutable
                        ),
                        null
                    )
                )
                .ReturnsAsync(31)
                .Verifiable(Times.Once());
        }

        // Act
        var result = await Sut.ExecuteAsync(new DesktopLaunchBuildCommand(settings), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        result.Value.ShouldBe(31);
        Mock.Mock<IDesktopCommandRunner>().Verify();
    }

    [Test]
    public async Task ShouldUseExpectedWindowsLaunchFallback_WhenWineIsUnavailable()
    {
        // Arrange
        const string publishedExecutable = "/repo/.artifacts/win-x64/publish/Reaparr.AppHost.exe";
        SetupFileSystem(system =>
        {
            system.AddDirectory("/repo/.artifacts/win-x64/publish");
            system.AddFile(publishedExecutable, new MockFileData("published"));
        });

        var settings = new DesktopCommandSettings { RuntimeIdentifier = "win-x64" };
        SetupDependencies(builder => builder.Register(_ => "/repo").As<string>().SingleInstance());

        if (OperatingSystem.IsWindows())
        {
            Mock.Mock<IDesktopCommandRunner>()
                .Setup(x =>
                    x.ExecuteCommandAsync(
                        publishedExecutable,
                        It.Is<IReadOnlyList<string>>(arguments => arguments.Count == 0),
                        null
                    )
                )
                .ReturnsAsync(0)
                .Verifiable(Times.Once());
        }
        else
        {
            Mock.Mock<IDesktopCommandRunner>()
                .Setup(x => x.CommandExistsAsync("wine"))
                .ReturnsAsync(false)
                .Verifiable(Times.Once());
        }

        // Act
        var result = await Sut.ExecuteAsync(new DesktopLaunchBuildCommand(settings), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        result.Value.ShouldBe(0);
        Mock.Mock<IDesktopCommandRunner>().Verify();
    }
}
