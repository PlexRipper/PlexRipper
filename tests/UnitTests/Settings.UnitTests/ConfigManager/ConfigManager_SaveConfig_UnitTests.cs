using System.Reactive.Subjects;
using Autofac;
using Autofac.Extras.Moq;
using Environment;
using FluentResults;
using Logging;
using Moq;
using PlexRipper.Application;
using PlexRipper.Settings;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Settings.UnitTests
{
    public class ConfigManager_SaveConfig_UnitTests
    {
        public ConfigManager_SaveConfig_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldLoadConfigDuringSetup_WhenConfigFileAlreadyExists()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<ISettingsModel>());
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns(() => "TEST_PlexRipperSettings.json");
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(() => "/");
            mock.Mock<IUserSettings>().Setup(x => x.GetJsonSettingsObject()).Returns(Result.Ok("{}"));
            mock.Mock<IFileSystem>().Setup(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>())).Returns(Result.Ok);

            var sut = new Mock<ConfigManager>(
                MockBehavior.Strict,
                mock.Container.Resolve<IFileSystem>(),
                mock.Container.Resolve<IPathProvider>(),
                mock.Container.Resolve<IUserSettings>());
            sut.Setup(x => x.SaveConfig()).CallBase();
            sut.Setup(x => x.ConfigFileExists()).Returns(true);
            sut.Setup(x => x.LoadConfig()).Returns(Result.Ok);

            // Act
            var resetResult = sut.Object.SaveConfig();

            // Assert
            resetResult.IsSuccess.ShouldBeTrue();
            mock.Mock<IFileSystem>().Verify(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }


    }
}