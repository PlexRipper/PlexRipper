using Autofac;
using Environment;
using FileSystem.Contracts;
using PlexRipper.Application;
using PlexRipper.Settings;
using Settings.Contracts;

namespace Settings.UnitTests;

public class ConfigManager_ResetConfig_UnitTests : BaseUnitTest<ConfigManager>
{
    public ConfigManager_ResetConfig_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ShouldReturnOkResult_WhenSettingsAreReset()
    {
        // Arrange
        mock.Mock<IUserSettings>().Setup(x => x.Reset());

        // Were mocking other methods from ConfigManager, that's why we need to mock it manually here
        var sut = new Mock<ConfigManager>(
            MockBehavior.Strict,
            mock.Container.Resolve<IFileSystem>(),
            mock.Container.Resolve<IDirectorySystem>(),
            mock.Container.Resolve<IPathProvider>(),
            mock.Container.Resolve<IUserSettings>());
        sut.Setup(x => x.SaveConfig()).Returns(Result.Ok);

        // Since ResetConfig is virtual we need to callBase here
        sut.Setup(x => x.ResetConfig()).CallBase();

        // Act
        var resetResult = sut.Object.ResetConfig();

        // Assert
        resetResult.IsSuccess.ShouldBeTrue();
        sut.Verify(x => x.SaveConfig(), Times.Once);
        mock.Mock<IUserSettings>().Verify(x => x.Reset(), Times.Once);
    }
}