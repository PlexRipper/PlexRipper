using System.Threading.Tasks;
using Logging;
using PlexRipper.BaseTests;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace BaseTests.IntegrationTests.Setup
{
    public class IntegrationTest_Setup
    {
        public IntegrationTest_Setup(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldHaveValidApiHttpClient_WhenStartingAnIntegrationTest()
        {
            var container = await BaseContainer.Create();
            container.ApiClient.ShouldNotBeNull();
        }

        [Fact]
        public async Task ShouldHaveAllResolvePropertiesValid_WhenStartingAnIntegrationTest()
        {
            var container = await BaseContainer.Create();
            container.FileSystem.ShouldNotBeNull();
            container.GetDownloadCommands.ShouldNotBeNull();
            container.GetDownloadManager.ShouldNotBeNull();
            container.GetDownloadQueue.ShouldNotBeNull();
            container.GetDownloadTaskFactory.ShouldNotBeNull();
            container.GetDownloadTaskValidator.ShouldNotBeNull();
            container.GetDownloadTracker.ShouldNotBeNull();
            container.GetFolderPathService.ShouldNotBeNull();
            container.GetPlexAccountService.ShouldNotBeNull();
            container.GetPlexApiService.ShouldNotBeNull();
            container.GetPlexDownloadService.ShouldNotBeNull();
            container.GetPlexLibraryService.ShouldNotBeNull();
            container.GetPlexRipperHttpClient.ShouldNotBeNull();
            container.GetPlexServerService.ShouldNotBeNull();
            container.Mediator.ShouldNotBeNull();
            container.PathProvider.ShouldNotBeNull();
            container.PlexRipperDbContext.ShouldNotBeNull();
        }

        [Fact]
        public async Task ShouldHaveUniqueInMemoryDatabase_WhenConfigFileIsGivenToContainer()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 9999,
                MemoryDbName = MockDatabase.GetMemoryDatabaseName(),
            };
            var container = await BaseContainer.Create(config);

            // Act
            var dbContext = container.PlexRipperDbContext;

            // Assert
            container.ShouldNotBeNull();
            dbContext.ShouldNotBeNull();
            dbContext.DatabaseName.ShouldBe(config.MemoryDbName);
        }
    }
}