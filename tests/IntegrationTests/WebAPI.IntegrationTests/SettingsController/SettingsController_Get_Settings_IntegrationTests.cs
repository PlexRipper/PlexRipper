using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Domain.Config;
using PlexRipper.Settings.Models;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace WebAPI.IntegrationTests.SettingsController
{
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
            var result = await response.Deserialize<ResultDTO<SettingsModelDTO>>();

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            result.IsSuccess.ShouldBeTrue();
            var responseSettings =
                JsonSerializer.Serialize(Container.Mapper.Map<SettingsModel>(result.Value), DefaultJsonSerializerOptions.ConfigBase);
            var defaultSettings = JsonSerializer.Serialize(new SettingsModel(), DefaultJsonSerializerOptions.ConfigBase);

            responseSettings.ShouldBe(defaultSettings);
        }
    }
}