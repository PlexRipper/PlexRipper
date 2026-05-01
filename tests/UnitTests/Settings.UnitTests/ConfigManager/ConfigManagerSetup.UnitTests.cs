using System.IO.Abstractions;
using System.Reactive.Subjects;
using Reaparr.Settings.Contracts;

namespace Reaparr.Settings.UnitTests;

public class ConfigManagerSetupUnitTests : BaseUnitTest<ConfigManager>
{
    [Test]
    public void ShouldLoadConfigDuringSetup_WhenConfigFileAlreadyExists()
    {
        // Arrange
        Mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        Mock.Mock<IDirectory>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        Mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(Mock.Mock<IDirectoryInfo>().Object)
            .Verifiable(Times.Never);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        Mock.Mock<IUserSettings>().Setup(x => x.Reset());

        // Act
        var resetResult = Sut.Setup();

        // Assert
        resetResult.IsSuccess.ShouldBeTrue();

        Mock.Mock<IUserSettings>().VerifyGet(x => x.SettingsUpdated, Times.Once);
    }

    [Test]
    public void ShouldCreateConfigFile_WhenConfigFileDoesNotExists()
    {
        // Arrange
        Mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        Mock.Mock<IFile>().Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Verifiable(Times.Once);

        Mock.Mock<IDirectory>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        Mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false);

        // Act
        var resetResult = Sut.Setup();

        // Assert
        resetResult.IsSuccess.ShouldBeTrue();
        Mock.Mock<IUserSettings>().VerifyGet(x => x.SettingsUpdated, Times.Once);
    }
}
