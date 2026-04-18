using System.Text.Json;
using Autofac;

namespace Reaparr.IntegrationTests;

public class UpdateEndpointsIntegrationTests : BaseIntegrationTests
{
    [Test]
    public async Task ShouldReturnFailureWithoutInvokingUpdateManager_WhenDownloadRequestedInDockerMode()
    {
        // Arrange
        using var environmentOverride = CreateEnvironmentOverride("docker");
        var seed = new Seed(12001);
        var updateManagerMock = new Mock<IUpdateManager>();

        using var container = await CreateContainer(
            seed,
            config =>
                config.OverrideServices = builder =>
                {
                    builder.RegisterInstance(updateManagerMock.Object).As<IUpdateManager>().SingleInstance();
                }
        );
        var client = container.GetApiClient();
        await client.SignIn();

        // Act
        var response = await client.PostAsync(ApiRoutes.UpdateController + "/download", null, CancellationToken);
        var result = await DeserializeBaseResultDTO(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        result.IsSuccess.ShouldBeFalse();
        result.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].Message.ShouldBe("Desktop updates are not supported in the current runtime mode");
        updateManagerMock.Verify(x => x.DownloadUpdateAsync(It.IsAny<CancellationToken>()), Times.Never);
        updateManagerMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ShouldReturnFailureWithoutInvokingUpdateManager_WhenApplyRequestedInDockerMode()
    {
        // Arrange
        using var environmentOverride = CreateEnvironmentOverride("docker");
        var seed = new Seed(12002);
        var updateManagerMock = new Mock<IUpdateManager>();

        using var container = await CreateContainer(
            seed,
            config =>
                config.OverrideServices = builder =>
                {
                    builder.RegisterInstance(updateManagerMock.Object).As<IUpdateManager>().SingleInstance();
                }
        );
        var client = container.GetApiClient();
        await client.SignIn();

        // Act
        var response = await client.PostAsync(ApiRoutes.UpdateController + "/execute", null, CancellationToken);
        var result = await DeserializeBaseResultDTO(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        result.IsSuccess.ShouldBeFalse();
        result.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].Message.ShouldBe("Desktop updates are not supported in the current runtime mode");
        updateManagerMock.Verify(x => x.ApplyUpdateAndRestart(), Times.Never);
        updateManagerMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ShouldReturnSuccessAndInvokeUpdateManager_WhenDownloadRequestedInDesktopMode()
    {
        // Arrange
        using var environmentOverride = OverrideEnvironmentVariables(
            new Dictionary<string, string?>
            {
                [EnvKeys.ReaparrPlatform] = "desktop",
                [EnvKeys.InformationalVersion] = "0.0.0",
            }
        );
        var seed = new Seed(12003);
        var updateManagerMock = new Mock<IUpdateManager>();
        updateManagerMock.Setup(x => x.DownloadUpdateAsync(It.IsAny<CancellationToken>())).ReturnsAsync("9.9.9");

        using var container = await CreateContainer(
            seed,
            config => ConfigureDesktopUpdateManager(config, updateManagerMock)
        );
        var client = container.GetApiClient();
        await client.SignIn();

        // Act
        var response = await client.PostAsync(ApiRoutes.UpdateController + "/download", null, CancellationToken);
        var result = await DeserializeBaseResultDTO(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.IsSuccess.ShouldBeTrue();
        result.StatusCode.ShouldBe(StatusCodes.Status200OK);
        result.Errors.ShouldBeEmpty();
        updateManagerMock.Verify(x => x.DownloadUpdateAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldReturnSuccessAndInvokeUpdateManager_WhenApplyRequestedInDesktopMode()
    {
        // Arrange
        using var environmentOverride = OverrideEnvironmentVariables(
            new Dictionary<string, string?>
            {
                [EnvKeys.ReaparrPlatform] = "desktop",
                [EnvKeys.InformationalVersion] = "0.0.0",
            }
        );
        var seed = new Seed(12004);
        var updateManagerMock = new Mock<IUpdateManager>();

        using var container = await CreateContainer(
            seed,
            config =>
                config.OverrideServices = builder =>
                {
                    builder.RegisterInstance(updateManagerMock.Object).As<IUpdateManager>().SingleInstance();
                }
        );
        var client = container.GetApiClient();
        await client.SignIn();

        // Act
        var response = await client.PostAsync(ApiRoutes.UpdateController + "/execute", null, CancellationToken);
        var result = await DeserializeBaseResultDTO(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.IsSuccess.ShouldBeTrue();
        result.StatusCode.ShouldBe(StatusCodes.Status200OK);
        result.Errors.ShouldBeEmpty();
        updateManagerMock.Verify(x => x.ApplyUpdateAndRestart(), Times.Once);
    }

    private static IDisposable CreateEnvironmentOverride(string platform) =>
        OverrideEnvironmentVariables(new Dictionary<string, string?> { [EnvKeys.ReaparrPlatform] = platform });

    private static void ConfigureDesktopUpdateManager(UnitTestDataConfig config, Mock<IUpdateManager> updateManagerMock)
    {
        config.OverrideServices = builder =>
        {
            builder.RegisterInstance(updateManagerMock.Object).As<IUpdateManager>().SingleInstance();
        };
    }

    private static async Task<BaseResultDTO> DeserializeBaseResultDTO(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<BaseResultDTO>(
            content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        )!;
    }
}
