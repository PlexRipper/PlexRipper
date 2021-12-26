using System.Threading.Tasks;
using Autofac.Extras.Moq;
using Logging;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Settings.Models;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace WebAPI.IntegrationTests.SettingsController
{
    public class SettingsController_Get_Settings_IntegrationTests
    {
        public SettingsController_Get_Settings_IntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldHaveDefaultSettings_OnFirstTimeBoot()
        {
            // Arrange
            // var mock = AutoMock.GetStrict().Mock<IFileSystem>();
            // mock.Setup(x => x.())
            // var _fileSystemMock = mock.Create<IFileSystem>();
            var config = new UnitTestDataConfig
            {
                Seed = 9999,
                MockFileSystem = null,
            };

            var container = new BaseContainer(config);

            // Act
            var response = await container.ApiClient.GetAsync(ApiRoutes.Settings.GetSettings);
            var result = await response.Deserialize<ResultDTO<SettingsModelDTO>>();

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            result.IsSuccess.ShouldBeTrue();
            var responseSettings = result.Value;
            var defaultSettings = new SettingsModel();

            responseSettings.FirstTimeSetup.ShouldBe(defaultSettings.FirstTimeSetup);
            responseSettings.ActiveAccountId.ShouldBe(defaultSettings.ActiveAccountId);
            responseSettings.DownloadSegments.ShouldBe(defaultSettings.DownloadSegments);
            responseSettings.Language.ShouldBe(defaultSettings.Language);

            responseSettings.AskDownloadMovieConfirmation.ShouldBe(defaultSettings.AskDownloadMovieConfirmation);
            responseSettings.AskDownloadTvShowConfirmation.ShouldBe(defaultSettings.AskDownloadTvShowConfirmation);
            responseSettings.AskDownloadSeasonConfirmation.ShouldBe(defaultSettings.AskDownloadSeasonConfirmation);
            responseSettings.AskDownloadEpisodeConfirmation.ShouldBe(defaultSettings.AskDownloadEpisodeConfirmation);

            responseSettings.TvShowViewMode.ShouldBe(defaultSettings.TvShowViewMode);
            responseSettings.MovieViewMode.ShouldBe(defaultSettings.MovieViewMode);

            responseSettings.ShortDateFormat.ShouldBe(defaultSettings.ShortDateFormat);
            responseSettings.LongDateFormat.ShouldBe(defaultSettings.LongDateFormat);
            responseSettings.TimeFormat.ShouldBe(defaultSettings.TimeFormat);
            responseSettings.TimeZone.ShouldBe(defaultSettings.TimeZone);
            responseSettings.ShowRelativeDates.ShouldBe(defaultSettings.ShowRelativeDates);
        }
    }
}