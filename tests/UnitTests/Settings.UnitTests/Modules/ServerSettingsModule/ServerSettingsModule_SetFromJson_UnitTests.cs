using System.Text.Json;
using FluentResults;
using PlexRipper.BaseTests;
using PlexRipper.Domain.DownloadManager;
using PlexRipper.Settings.Models;
using PlexRipper.Settings.Modules;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Settings.UnitTests.Modules
{
    public class ServerSettingsModule_SetFromJson_UnitTests : BaseUnitTest<ServerSettingsModule>
    {
        public ServerSettingsModule_SetFromJson_UnitTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void ShouldParseServerSettingsCorrectly_WhenGivenValidServerSettingsJson()
        {
            // Arrange
            var settingsModel = FakeData.GetSettingsModel(new UnitTestDataConfig(34)).Generate();
            var settingsModelJsonElement = FakeData.GetSettingsModelJsonElement(new UnitTestDataConfig(34));

            // Act
            var result = _sut.SetFromJson(settingsModelJsonElement);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            _sut.Data.Count.ShouldBe(5);
            foreach (var sourceServerSettingsModel in settingsModel.ServerSettings.Data)
            {
                var targetServerSettingsModel = _sut.Data.Find(x => x.PlexServerId == sourceServerSettingsModel.PlexServerId);
                targetServerSettingsModel.ShouldNotBeNull($"PlexServer with Id {sourceServerSettingsModel.PlexServerId} was not parsed correctly");
                targetServerSettingsModel.MachineIdentifier = sourceServerSettingsModel.MachineIdentifier;
                targetServerSettingsModel.DownloadSpeedLimit = sourceServerSettingsModel.DownloadSpeedLimit;
            }
        }
    }
}