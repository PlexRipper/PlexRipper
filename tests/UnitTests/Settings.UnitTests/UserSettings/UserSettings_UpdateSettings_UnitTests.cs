using System;
using System.Collections.Generic;
using Autofac.Extras.Moq;
using Logging;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Extensions;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Settings.UnitTests.UserSettings
{
    public class UserSettings_UpdateSettings_UnitTests
    {
        public UserSettings_UpdateSettings_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldUpdateSettings_WhenGivenValidSettingsModel()
        {
            // Arrange
            using var mock = AutoMock.GetStrict().AddMapper();
            var settingsModel = FakeData.GetSettingsModel().Generate();

            // Act
            var sut = mock.Create<PlexRipper.Settings.UserSettings>();
            var updateResult = sut.UpdateSettings(settingsModel);

            // Assert
            updateResult.IsSuccess.ShouldBeTrue();

           // sut.FirstTimeSetup.ShouldBe(settingsModel.FirstTimeSetup);
            sut.ActiveAccountId.ShouldBe(settingsModel.ActiveAccountId);
            sut.DownloadSegments.ShouldBe(settingsModel.DownloadSegments);

            sut.AskDownloadMovieConfirmation.ShouldBe(settingsModel.AskDownloadMovieConfirmation);
            sut.AskDownloadTvShowConfirmation.ShouldBe(settingsModel.AskDownloadTvShowConfirmation);
            sut.AskDownloadSeasonConfirmation.ShouldBe(settingsModel.AskDownloadSeasonConfirmation);
            sut.AskDownloadEpisodeConfirmation.ShouldBe(settingsModel.AskDownloadEpisodeConfirmation);

            sut.TvShowViewMode.ShouldBe(settingsModel.TvShowViewMode);
            sut.MovieViewMode.ShouldBe(settingsModel.MovieViewMode);

            sut.ShortDateFormat.ShouldBe(settingsModel.ShortDateFormat);
            sut.LongDateFormat.ShouldBe(settingsModel.LongDateFormat);
            sut.TimeFormat.ShouldBe(settingsModel.TimeFormat);
            sut.TimeZone.ShouldBe(settingsModel.TimeZone);
            sut.ShowRelativeDates.ShouldBe(settingsModel.ShowRelativeDates);
        }

        [Fact]
        public void ShouldEmitEvent_WhenSettingsAreUpdatedGivenValidSettingsModel()
        {
            // Arrange
            using var mock = AutoMock.GetStrict().AddMapper();
            var settingsModel = FakeData.GetSettingsModel().Generate();
            List<ISettingsModel> updates = new();

            // Act
            var sut = mock.Create<PlexRipper.Settings.UserSettings>();
            sut.SettingsUpdated.Subscribe(value => updates.Add(value));
            var updateResult = sut.UpdateSettings(settingsModel);

            // Assert
            updateResult.IsSuccess.ShouldBeTrue();
            updates.Count.ShouldBe(1);
        }
    }
}