using PlexRipper.Settings.Modules;

namespace Settings.UnitTests.Modules;

public class ServerSettingsModule_SetFromJson_UnitTests : BaseUnitTest<ServerSettingsModule>
{
    public ServerSettingsModule_SetFromJson_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldParseServerSettingsCorrectly_WhenGivenValidServerSettingsJson()
    {
        // Arrange
        var settingsModel = FakeData
            .GetSettingsModel(config =>
            {
                config.Seed = 34;
            })
            .Generate();
        var settingsModelJsonElement = FakeData.GetSettingsModelJsonElement(config =>
        {
            config.Seed = 34;
        });

        // Act
        var result = _sut.SetFromJson(settingsModelJsonElement);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _sut.Data.Count.ShouldBe(5);
        foreach (var sourceServerSettingsModel in settingsModel.ServerSettings.Data)
        {
            var targetServerSettingsModel = _sut.Data.Find(x =>
                x.MachineIdentifier == sourceServerSettingsModel.MachineIdentifier
            );
            targetServerSettingsModel.ShouldNotBeNull(
                $"PlexServer with {sourceServerSettingsModel.MachineIdentifier} was not parsed correctly"
            );
            targetServerSettingsModel.DownloadSpeedLimit = sourceServerSettingsModel.DownloadSpeedLimit;
        }
    }
}
