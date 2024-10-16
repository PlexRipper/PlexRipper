using System.Text.Json;
using Application.Contracts;
using FastEndpoints;
using PlexRipper.Application;
using PlexRipper.Domain.Config;
using PlexRipper.Settings;
using Settings.Contracts;

namespace IntegrationTests.WebAPI.SettingsController;

public class SettingsController_Get_Settings_IntegrationTests : BaseIntegrationTests
{
    public SettingsController_Get_Settings_IntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveDefaultSettings_OnFirstTimeBoot()
    {
        // Arrange
        using var Container = await CreateContainer(x => x.Seed = 4564);

        // Act
        var response = await Container.ApiClient.GETAsync<GetUserSettingsEndpoint, ResultDTO<SettingsModelDTO>>();
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
