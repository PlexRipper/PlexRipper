using System.IO.Abstractions;
using System.Reactive.Subjects;
using Autofac;
using Reaparr.Environment;
using Reaparr.Settings.Contracts;

namespace Reaparr.Settings.UnitTests;

public class ConfigManagerSaveConfigUnitTests : BaseUnitTest<ConfigManager>
{
    public ConfigManagerSaveConfigUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldLoadConfigDuringSetup_WhenConfigFileAlreadyExists()
    {
        // Arrange
        Mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns(() => "TEST_ReaparrSettings.json");
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(() => "/");
        Mock.Mock<IFile>().Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Verifiable(Times.Once);

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
        sut.Setup(x => x.SaveConfig()).CallBase();
        sut.Setup(x => x.ConfigFileExists()).Returns(true);
        sut.Setup(x => x.LoadConfig()).Returns(Result.Ok);

        // Act
        var resetResult = sut.Object.SaveConfig();

        // Assert
        resetResult.IsSuccess.ShouldBeTrue();
    }
}
