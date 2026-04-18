using Autofac;

namespace Reaparr.IntegrationTests;

public class CheckForUpdateEndpointIntegrationTests : BaseIntegrationTests
{
    [Test]
    public async Task ShouldReturnNoUpdate_WhenDockerModeAndCommandExecutorReturnsNoUpdate()
    {
        // Arrange
        using var environmentOverride = CreateEnvironmentOverride("docker", "0.38.0");
        var seed = new Seed(11001);

        using var container = await CreateContainer(
            seed,
            config =>
                config.OverrideServices = builder =>
                    builder
                        .Register(_ =>
                            new FakeCommandExecutor().Intercept<CheckForUpdatesCommand, Result<AppUpdateCheckResult>>(
                                (_, _) => Task.FromResult(Result.Ok(AppUpdateCheckResult.NoUpdate()))
                            )
                        )
                        .As<ICommandExecutor>()
                        .InstancePerDependency()
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
        using var environmentOverride = CreateEnvironmentOverride("docker", "0.38.0");
        var seed = new Seed(11002);
        var updateResult = AppUpdateCheckResult.UpdateAvailable(
            "0.39.0",
            [
                new ReleaseNote
                {
                    Version = "v0.39.0",
                    Notes = "Stable release notes",
                    ReleaseDate = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                    IsDevRelease = false,
                },
            ]
        );

        using var container = await CreateContainer(
            seed,
            config =>
                config.OverrideServices = builder =>
                    builder
                        .Register(_ =>
                            new FakeCommandExecutor().Intercept<CheckForUpdatesCommand, Result<AppUpdateCheckResult>>(
                                (_, _) => Task.FromResult(Result.Ok(updateResult))
                            )
                        )
                        .As<ICommandExecutor>()
                        .InstancePerDependency()
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
        using var environmentOverride = CreateEnvironmentOverride("docker", "0.38.0-dev.6");
        var seed = new Seed(11003);
        var updateResult = AppUpdateCheckResult.UpdateAvailable(
            "0.38.0-dev.7",
            [
                new ReleaseNote
                {
                    Version = "v0.38.0-dev.7",
                    Notes = "Development release notes",
                    ReleaseDate = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                    IsDevRelease = true,
                },
            ]
        );

        using var container = await CreateContainer(
            seed,
            config =>
                config.OverrideServices = builder =>
                    builder
                        .Register(_ =>
                            new FakeCommandExecutor().Intercept<CheckForUpdatesCommand, Result<AppUpdateCheckResult>>(
                                (_, _) => Task.FromResult(Result.Ok(updateResult))
                            )
                        )
                        .As<ICommandExecutor>()
                        .InstancePerDependency()
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
        dto.CurrentVersion.ShouldBe("0.38.0-dev.6");
        dto.ReleaseNotes.Count.ShouldBe(1);
        dto.ReleaseNotes[0].Version.ShouldBe("v0.38.0-dev.7");
        dto.ReleaseNotes[0].Notes.ShouldBe("Development release notes");
        dto.ReleaseNotes[0].IsDevRelease.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnUpdateAvailable_WhenDesktopModeAndCommandExecutorReturnsUpdate()
    {
        // Arrange
        using var environmentOverride = OverrideEnvironmentVariables(
            new Dictionary<string, string?>
            {
                [EnvKeys.ReaparrPlatform] = "desktop",
                [EnvKeys.InformationalVersion] = "0.38.0",
            }
        );
        var seed = new Seed(11004);
        var updateResult = AppUpdateCheckResult.UpdateAvailable(
            "9.9.9",
            [
                new ReleaseNote
                {
                    Version = "9.9.9",
                    Notes = "Desktop release notes",
                    ReleaseDate = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                    IsDevRelease = false,
                },
            ]
        );

        using var container = await CreateContainer(
            seed,
            config =>
                config.OverrideServices = builder =>
                {
                    builder
                        .Register(_ =>
                            new FakeCommandExecutor().Intercept<CheckForUpdatesCommand, Result<AppUpdateCheckResult>>(
                                (_, _) => Task.FromResult(Result.Ok(updateResult))
                            )
                        )
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
        using var environmentOverride = OverrideEnvironmentVariables(
            new Dictionary<string, string?>
            {
                [EnvKeys.ReaparrPlatform] = "desktop",
                [EnvKeys.InformationalVersion] = "0.38.0",
            }
        );
        var seed = new Seed(11005);

        using var container = await CreateContainer(
            seed,
            config =>
                config.OverrideServices = builder =>
                {
                    builder
                        .Register(_ =>
                            new FakeCommandExecutor().Intercept<CheckForUpdatesCommand, Result<AppUpdateCheckResult>>(
                                (_, _) => Task.FromResult(Result.Ok(AppUpdateCheckResult.NoUpdate()))
                            )
                        )
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

    private static IDisposable CreateEnvironmentOverride(string platform, string version) =>
        OverrideEnvironmentVariables(
            new Dictionary<string, string?>
            {
                [EnvKeys.ReaparrPlatform] = platform,
                [EnvKeys.InformationalVersion] = version,
            }
        );
}
