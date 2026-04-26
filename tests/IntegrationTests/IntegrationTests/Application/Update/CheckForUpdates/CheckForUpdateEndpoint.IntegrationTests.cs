using Autofac;

namespace Reaparr.IntegrationTests;

public class CheckForUpdateEndpointIntegrationTests : BaseIntegrationTests
{
    [Test]
    public async Task ShouldReturnNoUpdate_WhenDockerModeAndCommandExecutorReturnsNoUpdate()
    {
        // Arrange
        var seed = new Seed(11001);

        var updateResult = new AppUpdateCheckResult
        {
            IsUpdateAvailable = false,
            NewestVersion = "0.38.0",
            CurrentVersion = "0.38.0",
            ReleaseNotes = [],
        };

        using var container = await CreateContainer(
            seed,
            config =>
            {
                config.OverrideAppBuildInfo = new MockAppBuildInfo
                {
                    RuntimeMode = "docker",
                    Version = "0.38.0",
                    RuntimeIdentifier = "linux-x64",
                    CurrentOS = OperatingSystemPlatform.Linux,
                };
                config.OverrideServices = builder =>
                {
                    builder
                        .Register(_ => CreateCommandExecutor(updateResult))
                        .As<ICommandExecutor>()
                        .InstancePerDependency();
                };
            }
        );

        var client = container.GetApiClient();
        await client.SignIn();

        // Act
        var response = await client.GETAsync<CheckForUpdateEndpoint, ResultDTO<AppUpdateCheckDTO>>();

        // Assert
        response.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Result.IsSuccess.ShouldBeTrue();
        response.Result.Errors.ShouldBeEmpty();

        var dto = response.Result.Value.ShouldNotBeNull();
        dto.IsUpdateAvailable.ShouldBeFalse();
        dto.NewestVersion.ShouldBe("0.38.0");
        dto.CurrentVersion.ShouldBe("0.38.0");
        dto.ReleaseNotes.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldReturnUpdateAvailable_WhenDockerModeAndCommandExecutorReturnsStableUpdate()
    {
        // Arrange
        var seed = new Seed(11002);
        var updateResult = new AppUpdateCheckResult
        {
            IsUpdateAvailable = true,
            NewestVersion = "0.39.0",
            CurrentVersion = "0.38.0",
            ReleaseNotes =
            [
                new ReleaseNote
                {
                    Version = "v0.39.0",
                    Notes = "Stable release notes",
                    ReleaseDate = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                    IsDevRelease = false,
                },
            ],
        };

        using var container = await CreateContainer(
            seed,
            config =>
            {
                config.OverrideAppBuildInfo = new MockAppBuildInfo
                {
                    RuntimeMode = "docker",
                    Version = "0.38.0",
                    RuntimeIdentifier = "linux-x64",
                    CurrentOS = OperatingSystemPlatform.Linux,
                };
                config.OverrideServices = builder =>
                    builder
                        .Register(_ => CreateCommandExecutor(updateResult))
                        .As<ICommandExecutor>()
                        .InstancePerDependency();
            }
        );

        var client = container.GetApiClient();
        await client.SignIn();

        // Act
        var response = await client.GETAsync<CheckForUpdateEndpoint, ResultDTO<AppUpdateCheckDTO>>();

        // Assert
        response.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Result.IsSuccess.ShouldBeTrue();
        response.Result.Errors.ShouldBeEmpty();

        var dto = response.Result.Value.ShouldNotBeNull();
        dto.IsUpdateAvailable.ShouldBeTrue();
        dto.NewestVersion.ShouldBe("0.39.0");
        dto.CurrentVersion.ShouldBe("0.38.0");
        dto.ReleaseNotes.Count.ShouldBe(1);
        dto.ReleaseNotes[0].Version.ShouldBe("v0.39.0");
        dto.ReleaseNotes[0].Notes.ShouldBe("Stable release notes");
        dto.ReleaseNotes[0].IsDevRelease.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldReturnUpdateAvailable_WhenDockerModeAndCommandExecutorReturnsDevUpdate()
    {
        // Arrange
        var seed = new Seed(11003);
        var updateResult = new AppUpdateCheckResult
        {
            IsUpdateAvailable = true,
            NewestVersion = "0.38.0-dev.7",
            CurrentVersion = "0.38.0",
            ReleaseNotes =
            [
                new ReleaseNote
                {
                    Version = "v0.38.0-dev.7",
                    Notes = "Development release notes",
                    ReleaseDate = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                    IsDevRelease = true,
                },
            ],
        };

        using var container = await CreateContainer(
            seed,
            config =>
            {
                config.OverrideAppBuildInfo = new MockAppBuildInfo
                {
                    RuntimeMode = "docker",
                    Version = "0.38.0-dev.6",
                    RuntimeIdentifier = "linux-x64",
                    CurrentOS = OperatingSystemPlatform.Linux,
                };
                config.OverrideServices = builder =>
                    builder
                        .Register(_ => CreateCommandExecutor(updateResult))
                        .As<ICommandExecutor>()
                        .InstancePerDependency();
            }
        );

        var client = container.GetApiClient();
        await client.SignIn();

        // Act
        var response = await client.GETAsync<CheckForUpdateEndpoint, ResultDTO<AppUpdateCheckDTO>>();

        // Assert
        response.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Result.IsSuccess.ShouldBeTrue();
        response.Result.Errors.ShouldBeEmpty();

        var dto = response.Result.Value.ShouldNotBeNull();
        dto.IsUpdateAvailable.ShouldBeTrue();
        dto.NewestVersion.ShouldBe("0.38.0-dev.7");
        dto.CurrentVersion.ShouldBe("0.38.0");
        dto.ReleaseNotes.Count.ShouldBe(1);
        dto.ReleaseNotes[0].Version.ShouldBe("v0.38.0-dev.7");
        dto.ReleaseNotes[0].Notes.ShouldBe("Development release notes");
        dto.ReleaseNotes[0].IsDevRelease.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnUpdateAvailable_WhenDesktopModeAndCommandExecutorReturnsUpdate()
    {
        // Arrange
        var seed = new Seed(11004);
        var updateResult = new AppUpdateCheckResult
        {
            IsUpdateAvailable = true,
            NewestVersion = "9.9.9",
            CurrentVersion = "0.38.0",
            ReleaseNotes =
            [
                new ReleaseNote
                {
                    Version = "9.9.9",
                    Notes = "Desktop release notes",
                    ReleaseDate = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                    IsDevRelease = false,
                },
            ],
        };

        using var container = await CreateContainer(
            seed,
            config =>
            {
                config.OverrideAppBuildInfo = new MockAppBuildInfo
                {
                    RuntimeMode = "desktop",
                    Version = "0.38.0",
                    RuntimeIdentifier = "linux-x64",
                    CurrentOS = OperatingSystemPlatform.Linux,
                };
                config.OverrideServices = builder =>
                    builder
                        .Register(_ => CreateCommandExecutor(updateResult))
                        .As<ICommandExecutor>()
                        .InstancePerDependency();
            }
        );

        var client = container.GetApiClient();
        await client.SignIn();

        // Act
        var response = await client.GETAsync<CheckForUpdateEndpoint, ResultDTO<AppUpdateCheckDTO>>();

        // Assert
        response.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Result.IsSuccess.ShouldBeTrue();
        response.Result.Errors.ShouldBeEmpty();

        var dto = response.Result.Value.ShouldNotBeNull();
        dto.IsUpdateAvailable.ShouldBeTrue();
        dto.NewestVersion.ShouldBe("9.9.9");
        dto.CurrentVersion.ShouldBe("0.38.0");
        dto.ReleaseNotes.Count.ShouldBe(1);
        dto.ReleaseNotes[0].Version.ShouldBe("9.9.9");
        dto.ReleaseNotes[0].Notes.ShouldBe("Desktop release notes");
        dto.ReleaseNotes[0].IsDevRelease.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldReturnNoUpdate_WhenDesktopModeAndCommandExecutorReturnsNoUpdate()
    {
        // Arrange
        var seed = new Seed(11005);

        var updateResult = new AppUpdateCheckResult
        {
            IsUpdateAvailable = false,
            NewestVersion = "0.38.0",
            CurrentVersion = "0.38.0",
            ReleaseNotes = [],
        };

        using var container = await CreateContainer(
            seed,
            config =>
            {
                config.OverrideAppBuildInfo = new MockAppBuildInfo
                {
                    RuntimeMode = "desktop",
                    Version = "0.38.0",
                    RuntimeIdentifier = "linux-x64",
                    CurrentOS = OperatingSystemPlatform.Linux,
                };
                config.OverrideServices = builder =>
                    builder
                        .Register(_ => CreateCommandExecutor(updateResult))
                        .As<ICommandExecutor>()
                        .InstancePerDependency();
            }
        );

        var client = container.GetApiClient();
        await client.SignIn();

        // Act
        var response = await client.GETAsync<CheckForUpdateEndpoint, ResultDTO<AppUpdateCheckDTO>>();

        // Assert
        response.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Result.IsSuccess.ShouldBeTrue();
        response.Result.Errors.ShouldBeEmpty();

        var dto = response.Result.Value.ShouldNotBeNull();
        dto.IsUpdateAvailable.ShouldBeFalse();
        dto.NewestVersion.ShouldBe("0.38.0");
        dto.CurrentVersion.ShouldBe("0.38.0");
        dto.ReleaseNotes.ShouldBeEmpty();
    }

    private static FakeCommandExecutor CreateCommandExecutor(AppUpdateCheckResult updateResult) =>
        new FakeCommandExecutor()
            .Intercept<GetGitHubReleasesCommand, Result<IReadOnlyList<ReleaseNote>>>(
                (_, _) => Task.FromResult(Result.Ok<IReadOnlyList<ReleaseNote>>(updateResult.ReleaseNotes))
            )
            .Intercept<CheckForUpdatesCommand, Result<AppUpdateCheckResult>>(
                (_, _) => Task.FromResult(Result.Ok(updateResult))
            );
}
