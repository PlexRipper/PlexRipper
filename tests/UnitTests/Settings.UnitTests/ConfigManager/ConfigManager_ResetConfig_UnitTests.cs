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
    public class ConfigManager_ResetConfig_UnitTests
    {
        public ConfigManager_ResetConfig_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldReturnOkResult_WhenSettingsAreReset()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IUserSettings>().Setup(x => x.Reset());

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
}