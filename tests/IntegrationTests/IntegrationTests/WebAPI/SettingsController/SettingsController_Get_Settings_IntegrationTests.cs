using System.Text.Json;
using Application.Contracts;
using FastEndpoints;
using PlexRipper.Application;
using PlexRipper.Domain.Config;
using PlexRipper.Settings;
using Settings.Contracts;

namespace IntegrationTests.WebAPI.SettingsController;

public class SettingsControllerGetSettingsIntegrationTests : BaseIntegrationTests
{
    public SettingsControllerGetSettingsIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveDefaultSettings_OnFirstTimeBoot()
    {
        // Arrange
        using var container = await CreateContainer(4564);

        // Act
        var response = await container.ApiClient.GETAsync<GetUserSettingsEndpoint, ResultDTO<SettingsModelDTO>>();
        response.Response.IsSuccessStatusCode.ShouldBeTrue();

        // Assert
        var result = response.Result;
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        var settingsModel = result.Value.ToModel();
        var responseSettings = JsonSerializer.Serialize(settingsModel, DefaultJsonSerializerOptions.ConfigCapitalized);
        var defaultSettings = JsonSerializer.Serialize(
            new UserSettings(),
            DefaultJsonSerializerOptions.ConfigCapitalized
        );

        responseSettings.ShouldBe(defaultSettings);
    }
}
