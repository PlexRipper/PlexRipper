using System.Threading.Tasks;
using PlexRipper.BaseTests;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace BaseTests.IntegrationTests.Setup
{
    [Collection("Sequential")]
    public class IntegrationTest_Setup : BaseIntegrationTests
    {
        public IntegrationTest_Setup(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task ShouldHaveValidApiHttpClient_WhenStartingAnIntegrationTest()
        {
            await CreateContainer();
            Container.ApiClient.ShouldNotBeNull();
        }

        [Fact]
        public async Task ShouldHaveAllResolvePropertiesValid_WhenStartingAnIntegrationTest()
        {
            await CreateContainer();
            Container.FileSystem.ShouldNotBeNull();
            Container.GetDownloadCommands.ShouldNotBeNull();
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
        public async Task ShouldHaveUniqueInMemoryDatabase_WhenConfigFileIsGivenToContainer()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 9999,
                MemoryDbName = MockDatabase.GetMemoryDatabaseName(),
            };
            await CreateContainer(config);

            // Act
            var dbContext = Container.PlexRipperDbContext;

            // Assert
            Container.ShouldNotBeNull();
            dbContext.ShouldNotBeNull();
            dbContext.DatabaseName.ShouldBe(config.MemoryDbName);
        }

        [Fact]
        public async Task ShouldAllowForMultipleContainersToBeCreated_WhenMultipleAreCalled()
        {
            // Arrange
            await CreateContainer(new UnitTestDataConfig(3457));
            await CreateContainer(new UnitTestDataConfig(9654));

            // Act
            var dbContext = Container.PlexRipperDbContext;

            // Assert
            Container.ShouldNotBeNull();
            dbContext.ShouldNotBeNull();
        }
    }
}