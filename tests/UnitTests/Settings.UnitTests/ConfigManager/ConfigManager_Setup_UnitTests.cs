using System.IO.Abstractions;
using System.Reactive.Subjects;
using Environment;
using PlexRipper.Settings;
using Settings.Contracts;

namespace Settings.UnitTests;

public class ConfigManager_Setup_UnitTests : BaseUnitTest<ConfigManager>
{
    public ConfigManager_Setup_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldLoadConfigDuringSetup_WhenConfigFileAlreadyExists()
    {
        // Arrange
        mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns(() => "TEST_PlexRipperSettings.json");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(() => "/");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(() => "/TEST_PlexRipperSettings.json");
        mock.Mock<IDirectory>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true).Verifiable(Times.Once);
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(mock.Mock<IDirectoryInfo>().Object)
            .Verifiable(Times.Never);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true).Verifiable(Times.Once);
        mock.Mock<IUserSettings>().Setup(x => x.Reset());

        // Act
        var resetResult = _sut.Setup();

        // Assert
        resetResult.IsSuccess.ShouldBeTrue();

        mock.Mock<IUserSettings>().VerifyGet(x => x.SettingsUpdated, Times.Once);
    }

    [Fact]
    public void ShouldCreateConfigFile_WhenConfigFileDoesNotExists()
    {
        // Arrange
        mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns(() => "TEST_PlexRipperSettings.json");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(() => "/");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(() => "/TEST_PlexRipperSettings.json");

        mock.Mock<IFile>().Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Verifiable(Times.Once);

        mock.Mock<IDirectory>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false);

        // Act
        var resetResult = _sut.Setup();

        // Assert
        resetResult.IsSuccess.ShouldBeTrue();
        mock.Mock<IUserSettings>().VerifyGet(x => x.SettingsUpdated, Times.Once);
    }
}
