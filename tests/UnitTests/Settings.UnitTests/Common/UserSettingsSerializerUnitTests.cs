using PlexRipper.Settings;

namespace Settings.UnitTests.Common;

public class UserSettingsSerializerUnitTests : BaseUnitTest
{
    public UserSettingsSerializerUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData("")]
    [InlineData("{}")]
    public void ShouldSetAllDefaultValues_WhenAnEmptyJSONStringIsGiven(string json)
    {
        // Act
        var result = UserSettingsSerializer.Deserialize(json);

        // Assert
        result.ShouldNotBeNull();

        var defaultSettings = new SettingsModel();
        result.GeneralSettings.ShouldBe(defaultSettings.GeneralSettings);
        result.ConfirmationSettings.ShouldBe(defaultSettings.ConfirmationSettings);
        result.DateTimeSettings.ShouldBe(defaultSettings.DateTimeSettings);
        result.DisplaySettings.ShouldBe(defaultSettings.DisplaySettings);
        result.DownloadManagerSettings.ShouldBe(defaultSettings.DownloadManagerSettings);
        result.LanguageSettings.ShouldBe(defaultSettings.LanguageSettings);
        result.DebugSettings.ShouldBe(defaultSettings.DebugSettings);
        result.ServerSettings.Data.ShouldBeEmpty();
    }
}
