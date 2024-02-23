namespace IntegrationTests.BaseTests.Setup;

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
        Container.GetDownloadQueue.ShouldNotBeNull();
        Container.GetDownloadTaskFactory.ShouldNotBeNull();
        Container.GetDownloadTaskValidator.ShouldNotBeNull();
        Container.GetPlexApiService.ShouldNotBeNull();
        Container.Mediator.ShouldNotBeNull();
        Container.PathProvider.ShouldNotBeNull();
        Container.PlexRipperDbContext.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldHaveUniqueInMemoryDatabase_WhenConfigFileIsGivenToContainer()
    {
        // Arrange
        await CreateContainer(9999);

        // Act
        var dbContext = Container.PlexRipperDbContext;

        // Assert
        Container.ShouldNotBeNull();
        dbContext.ShouldNotBeNull();
        dbContext.DatabaseName.ShouldNotBeEmpty();
        dbContext.DatabaseName.ShouldContain("memory_database");
    }

    [Fact]
    public async Task ShouldAllowForMultipleContainersToBeCreated_WhenMultipleAreCalled()
    {
        // Arrange
        await CreateContainer(3457);
        await CreateContainer(9654);

        // Act
        var dbContext = Container.PlexRipperDbContext;

        // Assert
        Container.ShouldNotBeNull();
        dbContext.ShouldNotBeNull();
    }
}