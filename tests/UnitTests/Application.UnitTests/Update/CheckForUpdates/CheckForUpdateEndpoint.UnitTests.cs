using Reaparr.Environment;

namespace Reaparr.Application.UnitTests;

public class CheckForUpdateEndpointUnitTests : BaseUnitTest<CheckForUpdateEndpoint>
{
    private const string StableVersion = "0.38.0";
    private const string DevVersion = "0.38.0-dev.6";

    [Test]
    public async Task ShouldReturnSuccessResult_WhenTriggerSucceeds()
    {
        // Arrange
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act
        var endpoint = SetupEndpointUnitTest<CheckForUpdateEndpoint>();
        await endpoint.HandleAsync(CancellationToken);
        var result = endpoint.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken), Times.Once());
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenTriggerFails()
    {
        // Arrange
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken))
            .ReturnsAsync(Result.Fail("Scheduler error"))
            .Verifiable(Times.Once());

        // Act
        var endpoint = SetupEndpointUnitTest<CheckForUpdateEndpoint>();
        await endpoint.HandleAsync(CancellationToken);
        var result = endpoint.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();

        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken), Times.Once());
    }

    [Test]
    public async Task ShouldReturnNoUpdate_WhenDockerModeAndCommandExecutorReturnsNoUpdate()
    {
        // Arrange
        SetAppBuildInfo(x =>
        {
            x.RuntimeMode = "docker";
            x.InformationalVersion = StableVersion;
        });

        var noUpdate = new AppUpdateCheckResult
        {
            IsUpdateAvailable = false,
            NewestVersion = StableVersion,
            CurrentVersion = StableVersion,
            ReleaseNotes = [],
        };
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken))
            .ReturnsAsync(Result.Ok(noUpdate))
            .Verifiable(Times.Once());

        // Act
        var endpoint = SetupEndpointUnitTest<CheckForUpdateEndpoint>();
        await endpoint.HandleAsync(CancellationToken);
        var result = endpoint.Response as ResultDTO<AppUpdateCheckDTO>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value!.IsUpdateAvailable.ShouldBeFalse();
        result.Value.NewestVersion.ShouldBe(StableVersion);
        result.Value.CurrentVersion.ShouldBe(StableVersion);
        result.Value.ReleaseNotes.ShouldBeEmpty();

        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldReturnUpdateAvailable_WhenDockerModeAndCommandExecutorReturnsStableUpdate()
    {
        // Arrange
        SetAppBuildInfo(x =>
        {
            x.RuntimeMode = "docker";
            x.InformationalVersion = StableVersion;
        });

        var releaseNotes = new List<ReleaseNote>
        {
            new()
            {
                Version = "v0.39.0",
                Notes = "Stable release notes",
                ReleaseDate = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                IsDevRelease = false,
            },
        };
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken))
            .ReturnsAsync(
                Result.Ok(
                    new AppUpdateCheckResult
                    {
                        IsUpdateAvailable = true,
                        NewestVersion = "0.39.0",
                        CurrentVersion = StableVersion,
                        ReleaseNotes = releaseNotes,
                    }
                )
            )
            .Verifiable(Times.Once());

        // Act
        var endpoint = SetupEndpointUnitTest<CheckForUpdateEndpoint>();
        await endpoint.HandleAsync(CancellationToken);
        var result = endpoint.Response as ResultDTO<AppUpdateCheckDTO>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value!.IsUpdateAvailable.ShouldBeTrue();
        result.Value.NewestVersion.ShouldBe("0.39.0");
        result.Value.CurrentVersion.ShouldBe(StableVersion);
        result.Value.ReleaseNotes.Count.ShouldBe(1);
        result.Value.ReleaseNotes[0].Version.ShouldBe("v0.39.0");
        result.Value.ReleaseNotes[0].Notes.ShouldBe("Stable release notes");
        result.Value.ReleaseNotes[0].IsDevRelease.ShouldBeFalse();

        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldReturnUpdateAvailable_WhenDockerModeAndCommandExecutorReturnsDevUpdate()
    {
        // Arrange
        SetAppBuildInfo(x =>
        {
            x.RuntimeMode = "docker";
            x.InformationalVersion = DevVersion;
        });

        var releaseNotes = new List<ReleaseNote>
        {
            new()
            {
                Version = "v0.38.0-dev.7",
                Notes = "Development release notes",
                ReleaseDate = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                IsDevRelease = true,
            },
        };

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken))
            .ReturnsAsync(
                Result.Ok(
                    new AppUpdateCheckResult
                    {
                        IsUpdateAvailable = true,
                        NewestVersion = "0.38.0-dev.7",
                        CurrentVersion = DevVersion,
                        ReleaseNotes = releaseNotes,
                    }
                )
            )
            .Verifiable(Times.Once());

        // Act
        var endpoint = SetupEndpointUnitTest<CheckForUpdateEndpoint>();
        await endpoint.HandleAsync(CancellationToken);
        var result = endpoint.Response as ResultDTO<AppUpdateCheckDTO>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value!.IsUpdateAvailable.ShouldBeTrue();
        result.Value.NewestVersion.ShouldBe("0.38.0-dev.7");
        result.Value.CurrentVersion.ShouldBe(DevVersion);
        result.Value.ReleaseNotes.Count.ShouldBe(1);
        result.Value.ReleaseNotes[0].Version.ShouldBe("v0.38.0-dev.7");
        result.Value.ReleaseNotes[0].Notes.ShouldBe("Development release notes");
        result.Value.ReleaseNotes[0].IsDevRelease.ShouldBeTrue();

        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldReturnUpdateAvailable_WhenDesktopModeAndCommandExecutorReturnsUpdate()
    {
        // Arrange
        SetAppBuildInfo(x =>
        {
            x.RuntimeMode = "desktop";
            x.InformationalVersion = StableVersion;
        });

        var releaseNotes = new List<ReleaseNote>
        {
            new()
            {
                Version = "9.9.9",
                Notes = "Desktop release notes",
                ReleaseDate = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                IsDevRelease = false,
            },
        };

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken))
            .ReturnsAsync(
                Result.Ok(
                    new AppUpdateCheckResult
                    {
                        IsUpdateAvailable = true,
                        NewestVersion = "9.9.9",
                        CurrentVersion = StableVersion,
                        ReleaseNotes = releaseNotes,
                    }
                )
            )
            .Verifiable(Times.Once());

        // Act
        var endpoint = SetupEndpointUnitTest<CheckForUpdateEndpoint>();
        await endpoint.HandleAsync(CancellationToken);
        var result = endpoint.Response as ResultDTO<AppUpdateCheckDTO>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value!.IsUpdateAvailable.ShouldBeTrue();
        result.Value.NewestVersion.ShouldBe("9.9.9");
        result.Value.CurrentVersion.ShouldBe(StableVersion);
        result.Value.ReleaseNotes.Count.ShouldBe(1);
        result.Value.ReleaseNotes[0].Version.ShouldBe("9.9.9");
        result.Value.ReleaseNotes[0].Notes.ShouldBe("Desktop release notes");
        result.Value.ReleaseNotes[0].IsDevRelease.ShouldBeFalse();

        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldReturnNoUpdate_WhenDesktopModeAndCommandExecutorReturnsNoUpdate()
    {
        // Arrange
        SetAppBuildInfo(x =>
        {
            x.RuntimeMode = "desktop";
            x.InformationalVersion = StableVersion;
        });

        var noUpdate = new AppUpdateCheckResult
        {
            IsUpdateAvailable = false,
            NewestVersion = StableVersion,
            CurrentVersion = StableVersion,
            ReleaseNotes = [],
        };
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken))
            .ReturnsAsync(Result.Ok(noUpdate))
            .Verifiable(Times.Once());

        // Act
        var endpoint = SetupEndpointUnitTest<CheckForUpdateEndpoint>();
        await endpoint.HandleAsync(CancellationToken);
        var result = endpoint.Response as ResultDTO<AppUpdateCheckDTO>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value!.IsUpdateAvailable.ShouldBeFalse();
        result.Value.NewestVersion.ShouldBe(StableVersion);
        result.Value.CurrentVersion.ShouldBe(StableVersion);
        result.Value.ReleaseNotes.ShouldBeEmpty();

        Mock.Mock<ICommandExecutor>().Verify();
    }
}
