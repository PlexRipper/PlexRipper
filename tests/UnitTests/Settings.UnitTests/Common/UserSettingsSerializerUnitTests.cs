using PlexRipper.Settings;
using Settings.Contracts;

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
        result.GeneralSettingsModel.ShouldBeEquivalentTo(defaultSettings.GeneralSettingsModel);
        result.ConfirmationSettingsModel.ShouldBeEquivalentTo(defaultSettings.ConfirmationSettingsModel);
        result.DateTimeSettingsModel.ShouldBeEquivalentTo(defaultSettings.DateTimeSettingsModel);
        result.DisplaySettingsModel.ShouldBeEquivalentTo(defaultSettings.DisplaySettingsModel);
        result.DownloadManagerSettingsModel.ShouldBeEquivalentTo(defaultSettings.DownloadManagerSettingsModel);
        result.LanguageSettingsModel.ShouldBeEquivalentTo(defaultSettings.LanguageSettingsModel);
        result.DebugSettingsModel.ShouldBeEquivalentTo(defaultSettings.DebugSettingsModel);
        result.ServerSettingsModel.Data.ShouldBeEquivalentTo(defaultSettings.ServerSettingsModel.Data);
    }
}
