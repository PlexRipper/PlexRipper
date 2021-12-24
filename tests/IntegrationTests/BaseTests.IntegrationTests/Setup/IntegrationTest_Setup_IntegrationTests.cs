using Logging;
using PlexRipper.BaseTests;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace BaseTests.IntegrationTests.Setup
{
    public class IntegrationTest_Setup
    {
        private BaseContainer Container { get; }

        public IntegrationTest_Setup(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
            Container = new BaseContainer();
        }

        [Fact]
        public void ShouldHaveValidApiHttpClient_WhenStartingAnIntegrationTest()
        {
            Container.ApiClient.ShouldNotBeNull();
        }

        [Fact]
        public void ShouldHaveAllResolvePropertiesValid_WhenStartingAnIntegrationTest()
        {
            Container.FileSystem.ShouldNotBeNull();
            Container.GetDownloadCommands.ShouldNotBeNull();
            Container.GetDownloadManager.ShouldNotBeNull();
            Container.GetDownloadQueue.ShouldNotBeNull();
            Container.GetDownloadTaskFactory.ShouldNotBeNull();
            Container.GetDownloadTaskValidator.ShouldNotBeNull();
            Container.GetDownloadTracker.ShouldNotBeNull();
            Container.GetFolderPathService.ShouldNotBeNull();
            Container.GetPlexAccountService.ShouldNotBeNull();
            Container.GetPlexApiService.ShouldNotBeNull();
            Container.GetPlexDownloadService.ShouldNotBeNull();
            Container.GetPlexLibraryService.ShouldNotBeNull();
            Container.GetPlexRipperHttpClient.ShouldNotBeNull();
            Container.GetPlexServerService.ShouldNotBeNull();
            Container.Mediator.ShouldNotBeNull();
            Container.PathProvider.ShouldNotBeNull();
            Container.PlexRipperDbContext.ShouldNotBeNull();
        }

        [Fact]
        public void ShouldHaveUniqueInMemoryDatabase_WhenConfigFileIsGivenToContainer()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 9999,
                MemoryDbName = MockDatabase.GetMemoryDatabaseName(),
            };
            var container = new BaseContainer(config);

            // Act
            var dbContext = container.PlexRipperDbContext;

            // Assert
            container.ShouldNotBeNull();
            dbContext.ShouldNotBeNull();
            dbContext.DatabaseName.ShouldBe(config.MemoryDbName);
        }
    }
}