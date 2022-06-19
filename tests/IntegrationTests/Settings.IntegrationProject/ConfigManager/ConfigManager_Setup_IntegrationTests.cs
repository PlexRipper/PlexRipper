using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using Environment;
using FluentResults;
using Moq;
using PlexRipper.Application;
using Xunit;
using Xunit.Abstractions;
using PlexRipper.BaseTests;
using Shouldly;

namespace Settings.IntegrationProject.ConfigManager
{
    [Collection("Sequential")]
    public class ConfigManager_Setup_IntegrationTests : BaseIntegrationTests
    {
        public ConfigManager_Setup_IntegrationTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task ShouldCreateDefaultConfigFileOnStartup_WhenNoConfigFileExists()
        {
            // Arrange
            using AutoMock mock = AutoMock.GetStrict();
            mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Mock<IObservable<ISettingsModel>>().Object);
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST-PlexRipperSettings.json");
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns("/TEST-config/");
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns("/TEST-config/TEST-PlexRipperSettings.json");
            mock.Mock<IDirectorySystem>().Setup(x => x.Exists(It.IsAny<string>())).Returns(Result.Ok(true));

            mock.Mock<IFileSystem>().Setup(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>())).Returns(Result.Ok());
            mock.Mock<IFileSystem>().Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

            var sut = mock.Create<PlexRipper.Settings.ConfigManager>();

            // Act
            await CreateContainer(config =>
            {
                config.Seed = 4564;
                config.MockConfigManager = sut;
            });

            Container.ConfigManager.Setup();

            // Assert

            await Container.Boot.WaitForStartAsync(CancellationToken.None);

            Container.ApiClient.ShouldNotBeNull();
        }
    }
}