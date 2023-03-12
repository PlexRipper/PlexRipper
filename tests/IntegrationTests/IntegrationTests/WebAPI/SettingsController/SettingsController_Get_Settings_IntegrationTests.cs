using System.Text.Json;
using PlexRipper.Domain.Config;
using PlexRipper.Settings.Models;
using PlexRipper.WebAPI.Common;
using Settings.Contracts;

namespace IntegrationTests.WebAPI.SettingsController;

[Collection("Sequential")]
public class SettingsController_Get_Settings_IntegrationTests : BaseIntegrationTests
{
    public SettingsController_Get_Settings_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveDefaultSettings_OnFirstTimeBoot()
    {
        // Arrange
        await CreateContainer(4564);

        // Act
        var response = await Container.ApiClient.GetAsync(ApiRoutes.Settings.GetSettings);
        var result = await response.Deserialize<SettingsModelDTO>();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.IsSuccess.ShouldBeTrue();
        var settingsModel = Container.Mapper.Map<SettingsModel>(result.Value);
        var responseSettings = JsonSerializer.Serialize(settingsModel, DefaultJsonSerializerOptions.ConfigBase);
        var defaultSettings = JsonSerializer.Serialize(SettingsModel.DefaultSettings(), DefaultJsonSerializerOptions.ConfigBase);

        responseSettings.ShouldBe(defaultSettings);
    }
}