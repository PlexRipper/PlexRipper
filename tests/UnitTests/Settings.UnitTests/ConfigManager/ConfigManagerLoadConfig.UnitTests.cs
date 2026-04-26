using System.IO.Abstractions;
using Autofac;
using Reaparr.Environment;
using Reaparr.Settings.Contracts;

namespace Reaparr.Settings.UnitTests;

public class ConfigManagerLoadConfigUnitTests : BaseUnitTest<ConfigManager>
{
    [Test]
    public void ShouldLoadSettingsAndSendToUserSettings_WhenSettingsCanBeReadFromFile()
    {
        // Arrange
        var settingsModel = FakeData.GetSettingsModel(new Seed(89944)).Generate();
        var settingsJson = UserSettingsSerializer.Serialize(settingsModel);
        Mock.Mock<IFile>().Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(() => settingsJson);
        Mock.Mock<IUserSettings>().Setup(x => x.UpdateSettings(It.IsAny<UserSettings>())).Returns(settingsModel);

        // Act
        var loadResult = Sut.LoadConfig();

        // Assert
        loadResult.IsSuccess.ShouldBeTrue();
        Mock.Mock<IUserSettings>().Verify(x => x.Reset(), Times.Never);
    }

    [Test]
    public void ShouldResetSettings_WhenFailingToReadSettingsFromFile()
    {
        // Arrange
        Mock.Mock<IFile>().Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(() => "");
        Mock.Mock<IUserSettings>().Setup(x => x.Reset());

        // Were mocking other methods from ConfigManager, that's why we need to mock it manually here
        var sut = new Mock<ConfigManager>(
            MockBehavior.Strict,
            Mock.Container.Resolve<ILogger>(),
            Mock.Container.Resolve<IPathProvider>(),
            Mock.Container.Resolve<IUserSettings>(),
            Mock.Container.Resolve<IFile>(),
            Mock.Container.Resolve<IPath>(),
            Mock.Container.Resolve<IDirectory>()
        );
        sut.Setup(x => x.ResetConfig()).Returns(Result.Ok);
        sut.Setup(x => x.LoadConfig()).CallBase();

        // Act
        var loadResult = sut.Object.LoadConfig();

        // Assert
        loadResult.IsSuccess.ShouldBeTrue();
        sut.Verify(x => x.ResetConfig(), Times.Once);
    }

    [Test]
    public void ShouldResetSettingsWhenUserSettingsCouldNotBeSetFromJsonSerialization_WhenReadingInvalidParsedJsonSettings()
    {
        // Arrange
        Mock.Mock<IFile>().Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(() => "{}");

        // Were mocking other methods from ConfigManager, that's why we need to mock it manually here
        var sut = new Mock<ConfigManager>(
            MockBehavior.Strict,
            Mock.Container.Resolve<ILogger>(),
            Mock.Container.Resolve<IPathProvider>(),
            Mock.Container.Resolve<IUserSettings>(),
            Mock.Container.Resolve<IFile>(),
            Mock.Container.Resolve<IPath>(),
            Mock.Container.Resolve<IDirectory>()
        );
        sut.Setup(x => x.ResetConfig()).Returns(Result.Ok);
        sut.Setup(x => x.LoadConfig()).CallBase();

        // Act
        var loadResult = sut.Object.LoadConfig();

        // Assert
        loadResult.IsSuccess.ShouldBeTrue();
        sut.Verify(x => x.ResetConfig(), Times.Once);
    }

    [Test]
    public void ShouldResetSettingsWhenSerializationThrowsException_WhenReadingInvalidJsonSettings()
    {
        // Arrange
        Mock.Mock<IFile>().Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(() => "@#$%^&");
        Mock.Mock<IUserSettings>().Setup(x => x.Reset());

        // Were mocking other methods from ConfigManager, that's why we need to mock it manually here
        var sut = new Mock<ConfigManager>(
            MockBehavior.Strict,
            Mock.Container.Resolve<ILogger>(),
            Mock.Container.Resolve<IPathProvider>(),
            Mock.Container.Resolve<IUserSettings>(),
            Mock.Container.Resolve<IFile>(),
            Mock.Container.Resolve<IPath>(),
            Mock.Container.Resolve<IDirectory>()
        );
        sut.Setup(x => x.ResetConfig()).Returns(Result.Ok);
        sut.Setup(x => x.LoadConfig()).CallBase();

        // Act
        var loadResult = sut.Object.LoadConfig();

        // Assert
        loadResult.IsSuccess.ShouldBeTrue();
        sut.Verify(x => x.ResetConfig(), Times.Once);
    }
}
