using System.Text.Json;
using FastEndpoints;
using Reaparr.Application;
using Reaparr.Application.Contracts;
using Reaparr.Settings;
using Reaparr.Settings.Contracts;

namespace Reaparr.IntegrationTests.SettingsController;

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
        var client = container.GetApiClient();
        await client.SignIn();

        var response = await client.GETAsync<GetUserSettingsEndpoint, ResultDTO<SettingsModelDTO>>();
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
